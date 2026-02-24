using DotnetProject.Application.Features.TestExternal.Queries;
using DotnetProject.Application.Features.UserInfo.Queries.GetUserFile;
using DotnetProject.Core.Features.TestExternal.DTO;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.TestExternal
{ 
    public class AddProductEndpoint : Endpoint<AddProductQuery, ProductResponse>
    {

        public override void Configure()
        {
            Post("/api/testexternal/addProduct");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AddProductQuery req, CancellationToken ct)
        {
            var handler = Resolve<AddProductHandler>();
            var result = await handler.Handle(req, ct);

            await result.ToApiResponse(HttpContext, Logger);

        }
    }
}
