using DotnetProject.Core.Features.UserInfo.DTO;
using FeedCommonLib.Application.Abstractions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Features.Collaboration.Abstractions
{
    public interface ICollabRepository
    { 
        Task<Result<AxonsCollabDTO>> SaveGiveStar(AxonsCollabDTO data, CancellationToken ct = default); 
    }
}
