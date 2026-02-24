using DotnetProject.Application.Features.Common;
using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserFile
{
    internal class GetUserFileQueryValidator : TracedValidator<GetUserFileQuery>
    {

        public GetUserFileQueryValidator()
        {
            RuleFor(x => x.filename)
                .NotNull().WithMessage("File name cannot be null.")
                .NotEmpty().WithMessage("File name is required.")
                // เพิ่ม Must เพื่อตรวจสอบ whitespace
                .Must(filename => !string.IsNullOrWhiteSpace(filename))
                    .WithMessage("File name cannot be empty or whitespace.");
        }
    }
}
