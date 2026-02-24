using DotnetProject.Core.Shared;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Features.TestExternal.Operations
{
    public static class ProductFactory
    {
        public static async Task<Result<ProductDTO>> SetupProductReqData(string url, string Name, ProductData? Data,
           ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                StdResponse error = StdResponse.Create(FeatureErrors.UrlExternalNotFound);
                logger.Log(error, context: nameof(SetupProductReqData));
                return Result<ProductDTO>.Failure(error);
            }
            if (Data == null)
            {
                StdResponse error = StdResponse.Create(FeatureErrors.ProductIsNull);
                logger.Log(error, context: nameof(SetupProductReqData));
                return Result<ProductDTO>.Failure(error);
            }

            ProductDTO ret = new ProductDTO()
            {
                UrlRequest = url,
                RequestData = new ProductRequest() { Name = Name, Data = Data! },
            };
            return Result<ProductDTO>.Success(ret);
        }
    }
}
