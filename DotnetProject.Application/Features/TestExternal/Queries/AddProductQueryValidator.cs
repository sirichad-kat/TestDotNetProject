using DotnetProject.Application.Features.Common;
using FluentValidation;

namespace DotnetProject.Application.Features.TestExternal.Queries
{
    public class AddProductQueryValidator : TracedValidator<AddProductQuery>
    {

        public AddProductQueryValidator()
        {
            RuleFor(x => x.name)
                .NotNull().WithMessage("product name cannot be null.")
                .NotEmpty().WithMessage("product name is required.")
                // เพิ่ม Must เพื่อตรวจสอบ whitespace
                .Must(name => !string.IsNullOrWhiteSpace(name))
                    .WithMessage("product name cannot be empty or whitespace.");

            RuleFor(x => x.data)
                .NotNull().WithMessage("product data cannot be null.");

            RuleFor(x => x.data.Year)
                .NotNull().WithMessage("Year cannot be null.")
                .NotEmpty().WithMessage("Year is required.");
            RuleFor(x => x.data.Price)
                .NotNull().WithMessage("Price cannot be null.")
                .NotEmpty().WithMessage("Price is required.");
        }
    }
}
