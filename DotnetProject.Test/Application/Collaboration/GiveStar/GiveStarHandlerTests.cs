using DotnetProject.Application.Features.Collaboration.Commands.GiveStar;
using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Core.Features.UserInfo.DTO;
using DotnetProject.Infrastructure.Entities;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.Extensions.Logging;
using NSubstitute; 

namespace DotnetProject.Test.Application.Collaboration.GiveStar;

[TestFixture]
public class GiveStarHandlerTests
{
    private ICollabRepository _collabRepository;
    private ICollabViewReader _collabViewReader;
    private ILogger<GiveStarHandler> _logger;
    private GiveStarHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _collabRepository = Substitute.For<ICollabRepository>();
        _collabViewReader = Substitute.For<ICollabViewReader>();
        _logger = Substitute.For<ILogger<GiveStarHandler>>();
        _handler = new GiveStarHandler(_collabRepository, _collabViewReader, _logger);
    }

    private static GiveStarCommand CreateValidCommand() =>
        new(
            StarUser: "user.star",
            StarFullname: "Star User",
            Sprint: 1,
            Year: 2025,
            Remark: "Great job!",
            GivenUser: "user.given",
            GivenFullname: "Given User",
            SubTeam: "TeamA"
        );

    [Test]
    public async Task Handle_WhenValid_ShouldReturnSuccess()
    {
        var command = CreateValidCommand();
        var savedCollab = new AxonsCollabDTO { Id = 99 };

        _collabViewReader.GetNextValueAsync("axons_collab_id_seq", Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(99));
        _collabRepository.SaveGiveStar(Arg.Any<AxonsCollabDTO>(), Arg.Any<CancellationToken>())
            .Returns(Result<AxonsCollabDTO>.Success(savedCollab));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(99));
        Assert.That(result.Value.IsSuccess, Is.True);
    }

    [Test]
    public async Task Handle_WhenGetNextValueFails_ShouldReturnFailure()
    {
        var command = CreateValidCommand();
        var error = StdResponse.Create(Errors.Database);

        _collabViewReader.GetNextValueAsync("axons_collab_id_seq", Arg.Any<CancellationToken>())
            .Returns(Result<int>.Failure(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Handle_WhenSaveGiveStarFails_ShouldReturnFailure()
    {
        var command = CreateValidCommand();
        var error = StdResponse.Create(Errors.Database);

        _collabViewReader.GetNextValueAsync("axons_collab_id_seq", Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(99));
        _collabRepository.SaveGiveStar(Arg.Any<AxonsCollabDTO>(), Arg.Any<CancellationToken>())
            .Returns(Result<AxonsCollabDTO>.Failure(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Handle_WhenGivenUserSameAsStarUser_ShouldReturnFailure()
    {
        var command = CreateValidCommand() with { GivenUser = "user.star", StarUser = "user.star" };

        _collabViewReader.GetNextValueAsync("axons_collab_id_seq", Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(99));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }
}
