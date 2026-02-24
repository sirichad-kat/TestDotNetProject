using FeedCommonLib.Application.Abstractions.Tracing;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FluentValidation.Results;

namespace DotnetProject.Application.Features.Common
{
    public abstract class TracedValidator<T> : AbstractValidator<T>
    {
        private const string OperationTypeTag = "operation.type";
        private const string InputTypeTag = "input.type";
        private const string ErrorMsgTag = "error.message";
        private const string ErrorTypeTag = "error.type";

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
        {
            string operationName = $"Validate_{typeof(T).Name}";
            using Activity? activity = ActivitySourceProvider.Source.StartActivity(operationName);

            activity?.SetTag(OperationTypeTag, "Validation"); 
            activity?.SetTag(InputTypeTag, typeof(T).Name);
            var result = await base.ValidateAsync(context, cancellation);

            if (result.IsValid)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                activity?.SetStatus(ActivityStatusCode.Error, errorMessage);
                activity?.SetTag(ErrorMsgTag, errorMessage);
                activity?.SetTag(ErrorTypeTag, "ValidationFailure");
            }

            return result;
             
        }
    }
} 
