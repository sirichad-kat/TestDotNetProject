using DotnetProject.Application.Features.UserInfo.Queries.GetUserFile;
using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.Features.UserInfo;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.UserInfo
{ 
    public class GetUserFileEndpoint : Endpoint<GetUserFileQuery, ApiStreamResponse>
    {

        public override void Configure()
        {
            Get("/api/userinfo/download");
            AllowAnonymous(); 
        }

        public override async Task HandleAsync(GetUserFileQuery req, CancellationToken ct)
        {
            var handler = Resolve<GetUserFileHandler>();
            var result = await handler.Handle(req, ct);

            await result.ToApiStreamResponse(HttpContext, Logger);

        }
    }
}
