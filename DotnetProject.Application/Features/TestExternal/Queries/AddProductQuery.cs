using DotnetProject.Core.Features.TestExternal;
using DotnetProject.Core.Features.TestExternal.DTO;
using DotnetProject.Core.Features.UserInfo.DTO;
using FeedCommonLib.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.TestExternal.Queries
{ 
    public record AddProductQuery(string name, ProductData data) : IQueryRequest<ProductResponse>;
}
