using Dapper;
using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data.Common; 

namespace DotnetProject.Infrastructure.Postgresql.Queries
{
    public class CollabViewReader : ICollabViewReader
    {
        readonly PostgresqlApplicationDbContext context;
        public CollabViewReader(PostgresqlApplicationDbContext _context, IConfiguration configuration)
        {
            context = _context;
        }
        public async Task<Result<int>> GetNextValueAsync(string sequenceName, CancellationToken ct = default)
        {
              const string sql = @"SELECT nextval(@SequenceName)";

            try
            {
                DbConnection connection = context.Database.GetDbConnection();
                int result = await connection.QuerySingleAsync<int>(sql, new { SequenceName = sequenceName });
                return Result<int>.Success(Convert.ToInt32(result)); 
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(StdResponse.FromException(Errors.Database, ex)); 
            }

        }


    }
}
