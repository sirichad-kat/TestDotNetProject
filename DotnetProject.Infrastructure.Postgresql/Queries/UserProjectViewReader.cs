using Dapper;
using DotnetProject.Core.Features.UserInfo.Abstractions;
using DotnetProject.Core.Features.UserInfo.DTO;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace DotnetProject.Infrastructure.Postgresql.Queries
{
    public class UserProjectViewReader : IUserProjectViewReader
    {
        readonly PostgresqlApplicationDbContext context;
        private readonly ILogger<UserProjectViewReader> _logger;
        public UserProjectViewReader(PostgresqlApplicationDbContext _context, IConfiguration configuration,ILogger<UserProjectViewReader> logger)
        {
            context = _context;
            _logger = logger;
        }
        public async Task<Result<IEnumerable<ProjectRecord>>> GetProjectsByUserNameAsync(string? userName, CancellationToken ct = default)
        {
            try
            {
                const string sql = @"
            SELECT 
                p.year,
                p.project_code,
                p.project_name,
                p.sub_team, 
                m.member_id,
                m.username,
                m.full_name,
                m.email,
                m.role,
                m.department,
                m.squad_name 
            FROM axons_project p
            INNER JOIN axons_member m ON p.sub_team = m.squad_name
            WHERE m.username = @UserName";

                using (DbConnection connection = context.Database.GetDbConnection())
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();
                    IEnumerable<ProjectRecord> results = await connection.QueryAsync<ProjectRecord>(sql, new { UserName = userName });
                    return Result<IEnumerable<ProjectRecord>>.Success(results);
                }
            }
            catch (Exception ex)
            {
                StdResponse error = StdResponse.FromException(Errors.Database, ex);
                _logger.Log(error, context: nameof(GetProjectsByUserNameAsync), customMessage: $"Error occurred while fetching projects for user {userName}" ); 
                return Result<IEnumerable<ProjectRecord>>.Failure(error);
            }

        }
    }
}
