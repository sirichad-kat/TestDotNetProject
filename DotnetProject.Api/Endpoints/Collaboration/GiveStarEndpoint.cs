using DotnetProject.Application.Features.Collaboration.Commands.GiveStar; 
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.UserInfo
{
    public class GiveStarEndpoint : Endpoint<GiveStarCommand, GiveStarResultDto>
    {

        public override void Configure()
        {
            Post("/api/collaboration/givestar");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GiveStarCommand req, CancellationToken ct)
        { 
            GiveStarHandler handler = Resolve<GiveStarHandler>();
            Result<GiveStarResultDto> result = await handler.Handle(req, ct);
            await result.ToApiResponse( HttpContext,Logger);
             
        }
    }
}
