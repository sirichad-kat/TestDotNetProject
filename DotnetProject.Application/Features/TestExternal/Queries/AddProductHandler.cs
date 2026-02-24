
using DotnetProject.Application.Features.Collaboration.Commands.GiveStar;
using DotnetProject.Core.Features.TestExternal.Abstractions;
using DotnetProject.Core.Features.TestExternal.DTO;
using DotnetProject.Core.Features.TestExternal.Operations;
using DotnetProject.Core.Features.UserInfo.Abstractions;
using DotnetProject.Core.ServiceClient;
using FeedCommonLib.Application.Abstractions.Messaging;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Tracing;
using Microsoft.Extensions.Logging;

namespace DotnetProject.Application.Features.TestExternal.Queries
{ 
    public class AddProductHandler : IQueryRequestHandler<AddProductQuery, ProductResponse>
    {
        private readonly IFwInitViewReader _fwInitViewReader;
        private readonly IExternalServiceClient _externalService;
        private readonly ILogger<AddProductHandler> _logger;
        public AddProductHandler(IFwInitViewReader fwInitViewReader, IExternalServiceClient externalService, ILogger<AddProductHandler> logger)
        {
            _fwInitViewReader = fwInitViewReader;
            _externalService = externalService;
            _logger = logger;
        }
        public async Task<Result<ProductResponse>> Handle(AddProductQuery query, CancellationToken ct)
        {
            return await InstrumentedResultExtensions
               .BeginTracingAsync(async () => await _fwInitViewReader.GetInitValue("URL_PRODUCT", ct))
               .MapAsync(res => ProductFactory.SetupProductReqData(res,query.name,query.data, _logger))
               .ThenAsync(res => _externalService.CreateProductAsync(res))
               .Map(res => Result<ProductResponse>.Success(res, SuccessCodes.Created));
        }


    }
}
