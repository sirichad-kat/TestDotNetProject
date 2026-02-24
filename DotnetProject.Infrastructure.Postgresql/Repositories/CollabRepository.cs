
using AutoMapper;
using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Core.Features.UserInfo.DTO;
using DotnetProject.Infrastructure.Entities;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetProject.Infrastructure.Postgresql.Repositories
{
    public class CollabRepository : ICollabRepository
    {
        readonly PostgresqlApplicationDbContext context;
        private readonly ILogger<CollabRepository> logger;
        private readonly IMapper mapper;

        public CollabRepository(PostgresqlApplicationDbContext _context, IMapper _mapper, ILogger<CollabRepository> _logger)
        {
            context = _context;
            logger = _logger;
            mapper = _mapper;
        }


        public async Task<Result<AxonsCollabDTO>> SaveGiveStar(AxonsCollabDTO data, CancellationToken ct = default)
        {
            try
            {
                // Map DTO → Entity
                AxonsCollab entity = mapper.Map<AxonsCollab>(data);

                await context.AxonsCollabs.AddAsync(entity, ct);
                await context.SaveChangesAsync(ct);

                return Result<AxonsCollabDTO>.Success(data);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                var error = StdResponse.FromException(Errors.UniqueConstraint, ex, data);
                logger.Log(error, context: nameof(SaveGiveStar));

                return Result<AxonsCollabDTO>.Failure(error);
            }
            catch (Exception ex)
            {
                return Result<AxonsCollabDTO>.Failure(StdResponse.FromException(Errors.Database, ex));
            }
        }

    }

}
