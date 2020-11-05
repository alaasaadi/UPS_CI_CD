using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UPS.Core.Events;
using UPS.Core.Models;
using UPS.Core.Utilities.Export;
using UPS.Core.Utilities.Pagination;

namespace UPS.Core.Handlers
{
    public interface IHandler<T> where T : IModel
    {
        Task<IEnumerable<T>> ReadPageAsync(int? PageNo);
        Task<T> ReadAsync(T Entity);
        Task<T> CreateAsync(T Entity);
        Task<bool> UpdateAsync(T ModifiedEntity, T OrginalEntity);
        Task<bool> DeleteAsync(T Entity);

        string Filter { get; }
        void SetFilter(T Entity);
        void ClearFilter();

        IPager Pager { get; }
        Task<Stream> ExportAsync(IExporter Exporter);
        void Cancel();

        event EventHandler<HandlerEventArgs> ProccessingDataStarted;
        event EventHandler<HandlerEventArgs> ProccessingDataSuccess;
        event EventHandler<HandlerEventArgs> ProccessingDataError;
        event EventHandler<HandlerEventArgs> ProccessingDataEnded;
    }
}
