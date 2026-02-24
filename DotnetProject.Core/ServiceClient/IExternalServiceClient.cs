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
    public interface IExternalServiceClient
    {
        Task<Result<ProductResponse>> CreateProductAsync(ProductDTO request); 
    }
}
