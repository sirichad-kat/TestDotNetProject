using DotnetProject.Application.Features.Collaboration.Commands.GiveStar;
using FluentValidation.TestHelper;

namespace DotnetProject.Test.Application.Collaboration.GiveStar;

[TestFixture]
public class GiveStarCommandValidatorTests
{
    private GiveStarCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new GiveStarCommandValidator();
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
    public async Task ValidCommand_ShouldNotHaveErrors()
    {
        var command = CreateValidCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task Year_WhenNull_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Year = null };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Test]
    public async Task Year_WhenZero_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Year = 0 };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Test]
    public async Task Year_WhenNegative_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Year = -1 };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Test]
    public async Task Sprint_WhenNull_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Sprint = null };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Sprint);
    }

    [Test]
    public async Task Sprint_WhenZero_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Sprint = 0 };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Sprint);
    }

    [Test]
    public async Task Sprint_WhenNegative_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Sprint = -1 };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Sprint);
    }

    [Test]
    public async Task StarUser_WhenNull_ShouldHaveError()
    {
        var command = CreateValidCommand() with { StarUser = null! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.StarUser);
    }

    [Test]
    public async Task StarUser_WhenEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { StarUser = "" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.StarUser);
    }

    [Test]
    public async Task GivenUser_WhenNull_ShouldHaveError()
    {
        var command = CreateValidCommand() with { GivenUser = null };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.GivenUser);
    }

    [Test]
    public async Task GivenUser_WhenEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { GivenUser = "" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.GivenUser);
    }
}
