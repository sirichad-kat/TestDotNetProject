
using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Core.Features.Collaboration.Operations;
using FeedCommonLib.Application.Abstractions.Messaging;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Tracing;
using Microsoft.Extensions.Logging;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    public class GiveStarHandler : ICommandRequestHandler<GiveStarCommand, GiveStarResultDto>
    {
        private readonly ICollabRepository _collabRepository;
        private readonly ICollabViewReader _collabViewReader;
        private readonly ILogger<GiveStarHandler> _logger;
        public GiveStarHandler(ICollabRepository collabRepository, ICollabViewReader collabViewReader, ILogger<GiveStarHandler> logger)
        {
            _collabRepository = collabRepository;
            _collabViewReader = collabViewReader;
            _logger = logger;
        }

        public async Task<Result<GiveStarResultDto>> Handle(GiveStarCommand command, CancellationToken ct)
        {
            return await InstrumentedResultExtensions
                .BeginTracingAsync(() => _collabViewReader.GetNextValueAsync("axons_collab_id_seq", ct))
                .Then(newId => CollabFactory.CreateCollab(
                    _logger,
                    newId,
                    command.Year,
                    command.Sprint,
                    command.GivenUser,
                    command.GivenFullname,
                    command.StarUser,
                    command.StarFullname,
                    command.SubTeam,
                    command.Remark))
                .ThenAsync(async saveData => await _collabRepository.SaveGiveStar(saveData, ct))
                .Map(res => Result<GiveStarResultDto>.Success(new GiveStarResultDto { IsSuccess = true, Id = res.Id }, SuccessCodes.Created));
        }

    }
}
