using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UPS.DataAccess.Models;

namespace UPS.DataAccess
{
    public class GorestClient : IApiClient, IDisposable
    {
        static readonly Uri apiUri = new Uri("https://gorest.co.in/public-api/");
        static readonly string apiToken = "fa114107311259f5f33e70a5d85de34a2499b4401da069af0b1d835cd5ec0d56";

        static HttpClient client = null;
        static GorestClient instance = null;

        public static GorestClient Instance
        {
            get
            {

                if (instance == null)
                {
                    instance = new GorestClient();
                }
                return instance;
            }
        }
        private GorestClient()
        {
            client = new HttpClient()
            {
                BaseAddress = apiUri,
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IResponse> RequestAsync(HttpMethod methodType, string subAddress, string jsonData = null, CancellationToken CancelToken = default)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = methodType,
                RequestUri = new Uri($"{client.BaseAddress}/{subAddress}"),
            };

            if (!string.IsNullOrWhiteSpace(jsonData)) request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using (var result = await client.SendAsync(request, CancelToken))
            {
                if (result.IsSuccessStatusCode)
                {
                    var responseString = await result.Content.ReadAsStringAsync();

                    IResponse response = JsonSerializer.Deserialize<Response>(responseString);

                    switch (response.Status)
                    {
                        case ResponseCode.OK:
                        case ResponseCode.Created:
                        case ResponseCode.SuccessWithNoBody:
                        case ResponseCode.NoChange:
                            return response;

                        case ResponseCode.DataValidationFailed:
                            {
                                List<ValidationError> errors = JsonSerializer.Deserialize<List<ValidationError>>(response.Data.ToString());
                                List<ArgumentException> exceptions = new List<ArgumentException>();

                                errors.ForEach(x => exceptions.Add(new ArgumentException(x.Message, x.Field)));
                                throw new AggregateException(exceptions);
                            }


                        case ResponseCode.AuthenticationFailed:
                        case ResponseCode.AuthorizationFailed:
                        case ResponseCode.BadRequest:
                        case ResponseCode.HttpMethodNotAllowed:
                        case ResponseCode.InternalServerError:
                        case ResponseCode.NotExist:
                            {
                                IError error = JsonSerializer.Deserialize<Error>(response.Data.ToString());
                                throw new InvalidOperationException(error.Message);
                            }


                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Request error, error code: {result.StatusCode}");
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
