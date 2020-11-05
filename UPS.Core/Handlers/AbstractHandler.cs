using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UPS.Core.Attributes;
using UPS.Core.Events;
using UPS.Core.Exceptions;
using UPS.Core.Models;
using UPS.Core.Utilities.Export;
using UPS.Core.Utilities.Pagination;
using UPS.DataAccess;
using UPS.DataAccess.Models;

namespace UPS.Core.Handlers
{
    public abstract class AbstractHandler<T> : IHandler<T> where T : IModel, new()
    {
        readonly IApiClient manager = GorestClient.Instance;
        IPager _pager;
        public IPager Pager { get => _pager ?? new Pager(); protected set => _pager = value; }
        public string Filter { get; protected set; }
        protected JsonSerializerOptions SerializerOptions
        {
            get
            {
                var serializer = new JsonSerializerOptions();
                serializer.Converters.Add(new JsonStringEnumConverter());
                return serializer;
            }
        }
        protected bool MuteReadPageEvents { get; set; }
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public void Cancel()
        {
            tokenSource.Cancel();
        }
        protected void ResetToken()
        {
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
        }

        #region Events
        public event EventHandler<HandlerEventArgs> ProccessingDataStarted;
        public event EventHandler<HandlerEventArgs> ProccessingDataSuccess;
        public event EventHandler<HandlerEventArgs> ProccessingDataError;
        public event EventHandler<HandlerEventArgs> ProccessingDataEnded;
        #endregion

        #region CRUD Methods
        protected abstract string GetApiSubAddress(T Entity, HandlerActionType actionType);
        public async virtual Task<T> ReadAsync(T Entity)
        {
            try
            {
                if (Entity == null) throw new ArgumentNullException();
                ProccessingDataStarted?.Invoke(this, new HandlerEventArgs(HandlerActionType.Read));
                IResponse response = await manager.RequestAsync(HttpMethod.Get, GetApiSubAddress(Entity, HandlerActionType.Read), cancelToken: tokenSource.Token);
                T result = JsonSerializer.Deserialize<T>(response.Data.ToString(), SerializerOptions);
                ProccessingDataSuccess?.Invoke(this, new HandlerEventArgs(HandlerActionType.Read));
                return result;
            }
            catch (OperationCanceledException)
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Read, "Cancled by user"));
                throw;
            }
            catch
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Read));
                ResetToken();
                throw;
            }
            finally
            {
                ProccessingDataEnded?.Invoke(this, new HandlerEventArgs(HandlerActionType.Read));
            }
        }
        public async virtual Task<IEnumerable<T>> ReadPageAsync(int? PageNo = 1)
        {
            try
            {
                if (!MuteReadPageEvents) ProccessingDataStarted?.Invoke(this, new HandlerEventArgs(HandlerActionType.ReadPage));
                IResponse response = await manager.RequestAsync(HttpMethod.Get, $"{GetApiSubAddress(new T(), HandlerActionType.ReadPage)}?page={PageNo}&{Filter}", cancelToken: tokenSource.Token);
                Pager = JsonSerializer.Deserialize<PagerWrapper>(response.Meta.ToString(), SerializerOptions).Pager;
                IEnumerable<T> result = JsonSerializer.Deserialize<IEnumerable<T>>(response.Data.ToString(), SerializerOptions);
                if (!MuteReadPageEvents) ProccessingDataSuccess?.Invoke(this, new HandlerEventArgs(HandlerActionType.ReadPage));
                return result;
            }
            catch (OperationCanceledException)
            {
                if (!MuteReadPageEvents) ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.ReadPage, "Cancled by user"));
                ResetToken();
                throw;
            }
            catch
            {
                if (!MuteReadPageEvents) ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.ReadPage));
                throw;
            }
            finally
            {
                if (!MuteReadPageEvents) ProccessingDataEnded?.Invoke(this, new HandlerEventArgs(HandlerActionType.ReadPage));
            }
        }
        public async virtual Task<T> CreateAsync(T Entity)
        {
            try
            {
                if (Entity == null) throw new ArgumentNullException();
                ProccessingDataStarted?.Invoke(this, new HandlerEventArgs(HandlerActionType.Create));
                string EntityAsJson = JsonSerializer.Serialize<T>(Entity, SerializerOptions);
                IResponse response = await manager.RequestAsync(HttpMethod.Post, $"{GetApiSubAddress(Entity, HandlerActionType.Create)}", EntityAsJson, tokenSource.Token);
                T result = JsonSerializer.Deserialize<T>(response.Data.ToString(), SerializerOptions);
                ProccessingDataSuccess?.Invoke(this, new HandlerEventArgs(HandlerActionType.Create));
                return result;
            }
            catch (OperationCanceledException)
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Create, "Cancled by user"));
                ResetToken();
                throw;
            }
            catch
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Create));
                throw;
            }
            finally
            {
                ProccessingDataEnded?.Invoke(this, new HandlerEventArgs(HandlerActionType.Create));
            }
        }
        public async virtual Task<bool> UpdateAsync(T ModifiedEntity, T OrginalEntity)
        {
            try
            {
                if (ModifiedEntity == null || OrginalEntity == null) throw new ArgumentNullException();
                ProccessingDataStarted?.Invoke(this, new HandlerEventArgs(HandlerActionType.Update));
                // Handling data concurrency by getting same record from DB and compare it to orignal one
                var dbEntity = await ReadAsync(ModifiedEntity);
                if (dbEntity.CompareTo(OrginalEntity) <= 0)
                {
                    string EntityAsJson = JsonSerializer.Serialize<T>(ModifiedEntity, SerializerOptions);
                    IResponse response = await manager.RequestAsync(HttpMethod.Put, $"{GetApiSubAddress(ModifiedEntity, HandlerActionType.Update)}", EntityAsJson, tokenSource.Token);
                    ProccessingDataSuccess?.Invoke(this, new HandlerEventArgs(HandlerActionType.Update));
                    return (response.Status == ResponseCode.OK);
                }
                else
                {
                    throw new DataConcurrencyException("The record has been changed in database, please refresh your data and modify again");
                }
            }
            catch (OperationCanceledException)
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Update, "Cancled by user"));
                ResetToken();
                throw;
            }
            catch
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Update));
                throw;
            }
            finally
            {
                ProccessingDataEnded?.Invoke(this, new HandlerEventArgs(HandlerActionType.Update));
            }
        }
        public async virtual Task<bool> DeleteAsync(T Entity)
        {
            try
            {
                if (Entity == null) throw new ArgumentNullException();
                ProccessingDataStarted?.Invoke(this, new HandlerEventArgs(HandlerActionType.Delete));
                IResponse response = await manager.RequestAsync(HttpMethod.Delete, $"{GetApiSubAddress(Entity, HandlerActionType.Update)}", cancelToken: tokenSource.Token);
                ProccessingDataSuccess?.Invoke(this, new HandlerEventArgs(HandlerActionType.Delete));
                return (response.Status == ResponseCode.SuccessWithNoBody);
            }
            catch (OperationCanceledException)
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Delete, "Cancled by user"));
                ResetToken();
                throw;
            }
            catch
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Delete));
                throw;
            }
            finally
            {
                ProccessingDataEnded?.Invoke(this, new HandlerEventArgs(HandlerActionType.Delete));
            }
        }
        #endregion

        #region Search
        public virtual void SetFilter(T Entity)
        {
            try
            {
                if (Entity == null) throw new ArgumentNullException();

                StringBuilder sb = new StringBuilder();

                var searchableProperties = typeof(T).GetProperties().Where(property => property.IsDefined(typeof(SearchableFieldAttribute)));

                foreach (var property in searchableProperties)
                {
                    var attribute = (SearchableFieldAttribute)Attribute.GetCustomAttribute(property, typeof(SearchableFieldAttribute));
                    var propertyValue = property.GetValue(Entity);

                    if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                    {
                        // Exclude default value of the property type based on "ExcludeDefaultValue"
                        if (attribute.ExcludeDefaultValue)
                        {
                            var defaultValue = (property.PropertyType != typeof(string)) ? Activator.CreateInstance(property.PropertyType) : string.Empty;
                            if (propertyValue.Equals(defaultValue))
                                continue;
                        }

                        // Exclude values which defined in "ExcludedValues" array and applied on property
                        bool isExcludedValue = attribute.ExcludedValues?.ToList()?.Contains(propertyValue) ?? false;

                        if (propertyValue != null && !isExcludedValue)
                        {
                            sb.Append(@$"{attribute.FieldName}={propertyValue}&");
                        }
                    }
                }

                Filter = sb.ToString();

            }
            catch
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.SetFilter));
                throw;
            }

        }
        public void ClearFilter()
        {
            Filter = string.Empty;
        }
        #endregion

        #region Export
        public virtual async Task<Stream> ExportAsync(IExporter exporter)
        {
            try
            {
                MuteReadPageEvents = true;
                ProccessingDataStarted?.Invoke(this, new HandlerEventArgs(HandlerActionType.Export));
                await ReadPageAsync(1); // refresh handler results with pager
                var tasks = Enumerable.Range(1, Pager.TotalPages).Select(i => ReadPageAsync(i));
                var tasksResult = await Task.WhenAll(tasks);
                var combinedList = tasksResult.SelectMany(list => list);
                var result = await exporter.ExportAsync<T>(combinedList);
                ProccessingDataSuccess?.Invoke(this, new HandlerEventArgs(HandlerActionType.Export));
                return result;
            }
            catch (OperationCanceledException)
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Export, "Cancled by user"));
                ResetToken();
                throw;
            }
            catch
            {
                ProccessingDataError?.Invoke(this, new HandlerEventArgs(HandlerActionType.Export));
                throw;
            }
            finally
            {
                ProccessingDataEnded?.Invoke(this, new HandlerEventArgs(HandlerActionType.Export));
                MuteReadPageEvents = false;
            }

        }
        #endregion
    }
}
