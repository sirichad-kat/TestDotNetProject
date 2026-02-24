using DotnetProject.Application.Features.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    public class GiveStarCommandValidator : TracedValidator<GiveStarCommand>
    {
        //validate input ของ API  ห้ามนำ message ไปใช้ display บน client เพราะจะมีปัญหาเรื่อง localization และชื่อ label
        public GiveStarCommandValidator() { 
            RuleFor(x => x)
                .NotNull().WithMessage("Data is required."); 
             
            RuleFor(x =>  x.Year)
                .NotNull().WithMessage("Year cannot be null.") 
                .NotEmpty().WithMessage("Year is required.")
                .GreaterThan(0).WithMessage("Year must be greater than 0.");

            RuleFor(x => x.Sprint)
                .NotNull().WithMessage("Sprint cannot be null.")
                .NotEmpty().WithMessage("Sprint is required.")
                .GreaterThan(0).WithMessage("Sprint must be greater than 0.");

            RuleFor(x => x.StarUser)
                .NotNull().WithMessage("StarUser cannot be null.")
                .NotEmpty().WithMessage("StarUser is required.");

            RuleFor(x => x.GivenUser)
                .NotNull().WithMessage("GivenUser cannot be null.")
                .NotEmpty().WithMessage("GivenUser is required.");
             
             

        }
    }
}
