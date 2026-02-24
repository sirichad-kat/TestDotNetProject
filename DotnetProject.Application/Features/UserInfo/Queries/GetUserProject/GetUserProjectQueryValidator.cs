using DotnetProject.Application.Features.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserProject
{
    public class GetUserProjectQueryValidator : TracedValidator<GetUserProjectQuery>
    {

        public GetUserProjectQueryValidator()
        {
            RuleFor(x => x.userName)
                .NotNull().WithMessage("UserName cannot be null.")
                .NotEmpty().WithMessage("UserName is required.")
                // เพิ่ม Must เพื่อตรวจสอบ whitespace
                .Must(userName => !string.IsNullOrWhiteSpace(userName))
                    .WithMessage("UserName cannot be empty or whitespace.");
        }
    }
}
