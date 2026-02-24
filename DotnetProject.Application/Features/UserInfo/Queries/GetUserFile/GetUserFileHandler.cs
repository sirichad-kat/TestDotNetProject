using DotnetProject.Core.Features.UserInfo.Operations;
using DotnetProject.Core.Shared;
using FeedCommonLib.Application.Abstractions.EndpointExtension;
using FeedCommonLib.Application.Abstractions.Messaging;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Tracing;
using Microsoft.Extensions.Logging;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserFile
{
    public class GetUserFileHandler : IQueryRequestHandler<GetUserFileQuery, ApiStreamResponse>
    {
        private readonly ILogger<GetUserFileHandler> _logger;
        public GetUserFileHandler(ILogger<GetUserFileHandler> logger)
        {
            _logger = logger;
        }
        public async Task<Result<ApiStreamResponse>> Handle(GetUserFileQuery query, CancellationToken ct)
        {
            return await InstrumentedResultExtensions
               .BeginTracingAsync(() => UserFileService.DownloadUserFile(query.filename!, _logger))
               .Map(res => UserFileService.CreateFileInfo(res));
        } 
    }
}
