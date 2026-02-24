using DotnetProject.Core.Features.UserInfo.DTO;
using FeedCommonLib.Application.Abstractions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Features.UserInfo.Abstractions
{
    public interface IUserProjectViewReader
    {
        Task<Result<IEnumerable<ProjectRecord>>> GetProjectsByUserNameAsync(string? userName, CancellationToken ct = default);
    }
}
