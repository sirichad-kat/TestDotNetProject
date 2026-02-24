using FeedCommonLib.Application.Abstractions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Features.TestExternal.Abstractions
{ 
    public interface IFwInitViewReader
    {
        Task<Result<string>> GetInitValue(string keyName, CancellationToken ct = default);
    }
} 