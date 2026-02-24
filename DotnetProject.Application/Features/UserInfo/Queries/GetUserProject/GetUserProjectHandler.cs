using DotnetProject.Application.Features.Collaboration.Commands.GiveStar;
using DotnetProject.Core.Features.UserInfo.Abstractions;
using DotnetProject.Core.Features.UserInfo.DTO;
using FeedCommonLib.Application.Abstractions.Messaging;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Tracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserProject
{
    public class GetUserProjectHandler : IQueryRequestHandler<GetUserProjectQuery, IEnumerable<ProjectRecord>>
    {
        private readonly IUserProjectViewReader _userProjectViewReader;
        public GetUserProjectHandler(IUserProjectViewReader userProjectViewReader)
        {
            _userProjectViewReader = userProjectViewReader;
        }
        public async Task<Result<IEnumerable<ProjectRecord>>> Handle(GetUserProjectQuery query, CancellationToken ct)
        { 
            return await InstrumentedResultExtensions
               .BeginTracingAsync(async () => await _userProjectViewReader.GetProjectsByUserNameAsync(query.userName, ct))
               .Map(res => Result<IEnumerable<ProjectRecord>>.Success(res, SuccessCodes.Ok));
        }

      
    }
}
