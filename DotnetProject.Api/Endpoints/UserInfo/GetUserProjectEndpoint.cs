using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.Features.UserInfo.DTO;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.UserInfo
{
    public class GetUserProjectEndpoint : Endpoint<GetUserProjectQuery, IEnumerable<ProjectRecord>>
    {

        public override void Configure()
        {
            Get("/api/userinfo/project");
            AllowAnonymous();
            // Execute the Validation :: GetUserProjectQueryValidator 
            //Validator<GetUserProjectQueryValidator>();
        }

        public override async Task HandleAsync(GetUserProjectQuery req, CancellationToken ct)
        {
            var handler = Resolve<GetUserProjectHandler>();
            var result = await handler.Handle(req, ct);

            await result.ToApiResponse(HttpContext, Logger);
             
        }
    }
}
