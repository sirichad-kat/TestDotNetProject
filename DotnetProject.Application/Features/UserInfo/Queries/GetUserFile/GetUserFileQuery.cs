using DotnetProject.Core.Features.UserInfo;
using DotnetProject.Core.Shared;
using FeedCommonLib.Application.Abstractions.EndpointExtension;
using FeedCommonLib.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserFile
{
    public record GetUserFileQuery(string? filename) : IQueryRequest<ApiStreamResponse>;
}
