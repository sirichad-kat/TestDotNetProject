using DotnetProject.Core.Features.UserInfo.DTO;
using FeedCommonLib.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserProject
{
    public record GetUserProjectQuery(string userName) : IQueryRequest<IEnumerable<ProjectRecord>>; 
}
