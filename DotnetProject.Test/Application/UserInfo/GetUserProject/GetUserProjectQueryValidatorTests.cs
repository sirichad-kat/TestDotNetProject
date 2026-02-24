using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using FluentValidation.TestHelper;

namespace DotnetProject.Test.Application.UserInfo.GetUserProject;

[TestFixture]
public class GetUserProjectQueryValidatorTests
{
    private GetUserProjectQueryValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetUserProjectQueryValidator();
    }

    [Test]
    public async Task ValidQuery_ShouldNotHaveErrors()
    {
        var query = new GetUserProjectQuery("john.doe");

        var result = await _validator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task UserName_WhenNull_ShouldHaveError()
    {
        var query = new GetUserProjectQuery(null!);

        var result = await _validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.userName);
    }

    [Test]
    public async Task UserName_WhenEmpty_ShouldHaveError()
    {
        var query = new GetUserProjectQuery("");

        var result = await _validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.userName);
    }

    [Test]
    public async Task UserName_WhenWhitespace_ShouldHaveError()
    {
        var query = new GetUserProjectQuery("   ");

        var result = await _validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.userName);
    }
}
