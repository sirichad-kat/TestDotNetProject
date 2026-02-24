using DotnetProject.Core.Features.TestExternal;
using DotnetProject.Core.Features.TestExternal.DTO;
using DotnetProject.Core.Features.UserInfo.DTO;
using DotnetProject.Core.Shared;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DotnetProject.Core.ServiceClient
{
    public class ExternalServiceClient : IExternalServiceClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ExternalServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<ProductResponse>> CreateProductAsync(ProductDTO request)
        { 
            try
            {
                var json = JsonSerializer.Serialize(request.RequestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(request.UrlRequest, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                ProductResponse? productResponse = JsonSerializer.Deserialize<ProductResponse>(responseString, _jsonOptions);
                if (productResponse == null)
                {
                    return Result<ProductResponse>.Failure(StdResponse.Create(Errors.Unexpected, customMessage: "Deserialization returned null."));
                }
                return Result<ProductResponse>.Success(productResponse);
            }
            catch (Exception ex)
            {
                return Result<ProductResponse>.Failure(StdResponse.FromException(Errors.Unexpected, ex));
            }
        }
         
    }
}
