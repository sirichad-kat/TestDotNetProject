using DotnetProject.Core.Features.Collaboration.Operations;
using DotnetProject.Core.Shared;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DotnetProject.Test.Core.Collaboration;

[TestFixture]
public class CollabFactoryTests
{
    private ILogger _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger>();
    }

    [Test]
    public void CreateCollab_WhenAllValid_ShouldReturnSuccess()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: "user.b", startFullname: "User B",
            subTeam: "TeamX", remark: "Good job");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(1));
        Assert.That(result.Value.Year, Is.EqualTo(2025));
        Assert.That(result.Value.Sprint, Is.EqualTo(1));
        Assert.That(result.Value.GivenUser, Is.EqualTo("user.a"));
        Assert.That(result.Value.StarUser, Is.EqualTo("user.b"));
        Assert.That(result.Value.GivenFullname, Is.EqualTo("User A"));
        Assert.That(result.Value.StarFullname, Is.EqualTo("User B"));
        Assert.That(result.Value.SubTeam, Is.EqualTo("TeamX"));
        Assert.That(result.Value.Remark, Is.EqualTo("Good job"));
    }

    [Test]
    public void CreateCollab_WhenValid_ShouldSetGivenDateToUtc()
    {
        var before = DateTime.UtcNow;

        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: "user.b", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        var after = DateTime.UtcNow;

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.GivenDate, Is.Not.Null);
        Assert.That(result.Value.GivenDate!.Value.Kind, Is.EqualTo(DateTimeKind.Unspecified));
        Assert.That(result.Value.GivenDate.Value, Is.InRange(before, after));
    }

    [Test]
    public void CreateCollab_WhenValid_OptionalFieldsCanBeNull()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: null,
            givenUser: "user.a", givenFullname: null,
            starUser: "user.b", startFullname: null,
            subTeam: null, remark: null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Sprint, Is.Null);
        Assert.That(result.Value.GivenFullname, Is.Null);
        Assert.That(result.Value.StarFullname, Is.Null);
        Assert.That(result.Value.SubTeam, Is.Null);
        Assert.That(result.Value.Remark, Is.Null);
    }

    #region GivenUser validation

    [Test]
    public void CreateCollab_WhenGivenUserIsNull_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: null, givenFullname: "User A",
            starUser: "user.b", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.GivenUserIsNull.Code));
    }

    [Test]
    public void CreateCollab_WhenGivenUserIsEmpty_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "", givenFullname: "User A",
            starUser: "user.b", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.GivenUserIsNull.Code));
    }

    [Test]
    public void CreateCollab_WhenGivenUserIsWhitespace_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "   ", givenFullname: "User A",
            starUser: "user.b", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.GivenUserIsNull.Code));
    }

    #endregion

    #region StarUser validation

    [Test]
    public void CreateCollab_WhenStarUserIsNull_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: null, startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.StarUserIsNull.Code));
    }

    [Test]
    public void CreateCollab_WhenStarUserIsEmpty_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: "", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.StarUserIsNull.Code));
    }

    [Test]
    public void CreateCollab_WhenStarUserIsWhitespace_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: "   ", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.StarUserIsNull.Code));
    }

    #endregion

    #region Self-star validation

    [Test]
    public void CreateCollab_WhenGivenUserEqualStarUser_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: 2025, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: "user.a", startFullname: "User A",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.CannotGiveStarToSelf.Code));
    }

    #endregion

    #region Id validation

    [Test]
    public void CreateCollab_WhenIdIsNull_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: null, year: 2025, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: "user.b", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.InvalidStarData.Code));
    }

    #endregion

    #region Year validation

    [Test]
    public void CreateCollab_WhenYearIsNull_ShouldReturnFailure()
    {
        var result = CollabFactory.CreateCollab(
            _logger, id: 1, year: null, sprint: 1,
            givenUser: "user.a", givenFullname: "User A",
            starUser: "user.b", startFullname: "User B",
            subTeam: "TeamX", remark: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo(FeatureErrors.InvalidStarData.Code));
    }

    #endregion
}
