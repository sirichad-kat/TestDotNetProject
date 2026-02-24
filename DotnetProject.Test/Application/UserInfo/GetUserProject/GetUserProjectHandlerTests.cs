using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.Features.UserInfo.Abstractions;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using NSubstitute;
using DotnetProject.Core.Features.UserInfo.DTO;

namespace DotnetProject.Test.Application.UserInfo.GetUserProject;

[TestFixture]
public class GetUserProjectHandlerTests
{
    private IUserProjectViewReader _userProjectViewReader;
    private GetUserProjectHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _userProjectViewReader = Substitute.For<IUserProjectViewReader>();
        _handler = new GetUserProjectHandler(_userProjectViewReader);
    }

    [Test]
    public async Task Handle_WhenProjectsExist_ShouldReturnSuccess()
    {
        var query = new GetUserProjectQuery("john.doe");
        var projects = new List<ProjectRecord>
        {
            new() { ProjectCode = "PRJ001", ProjectName = "Project A", Username = "john.doe", FullName = "John Doe", Year = 2025 },
            new() { ProjectCode = "PRJ002", ProjectName = "Project B", Username = "john.doe", FullName = "John Doe", Year = 2025 }
        };

        _userProjectViewReader.GetProjectsByUserNameAsync("john.doe", Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProjectRecord>>.Success(projects));

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_WhenNoProjects_ShouldReturnSuccessWithEmpty()
    {
        var query = new GetUserProjectQuery("john.doe");
        var projects = Enumerable.Empty<ProjectRecord>();

        _userProjectViewReader.GetProjectsByUserNameAsync("john.doe", Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProjectRecord>>.Success(projects));

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Any(), Is.False);
    }

    [Test]
    public async Task Handle_WhenReaderFails_ShouldReturnFailure()
    {
        var query = new GetUserProjectQuery("john.doe");
        var error = StdResponse.Create(Errors.Database);

        _userProjectViewReader.GetProjectsByUserNameAsync("john.doe", Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProjectRecord>>.Failure(error));

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }
}
