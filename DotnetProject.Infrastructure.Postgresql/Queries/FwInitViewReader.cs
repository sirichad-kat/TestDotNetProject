using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Core.Features.TestExternal.Abstractions;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DotnetProject.Infrastructure.Postgresql.Queries
{ 
    public class FwInitViewReader : IFwInitViewReader
    {
        readonly PostgresqlApplicationDbContext context;
        public FwInitViewReader(PostgresqlApplicationDbContext _context, IConfiguration configuration)
        {
            context = _context;
        }
        public async Task<Result<string>> GetInitValue(string keyName, CancellationToken ct = default)
        {
            try
            {
                // Fix: Use nullable string and check for null before returning Result<string>.Success
                string? ret = context.FwInits.FirstOrDefault(x => x.KeyName == keyName)?.Value; 
                return ret is not null
                    ? Result<string>.Success(ret)
                    : Result<string>.Failure(StdResponse.Create(Errors.Database, "Value not found."));
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(StdResponse.FromException(Errors.Database, ex));
            }

        }


    }
}
