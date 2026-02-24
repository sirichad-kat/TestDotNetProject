using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.Common
{
    public class BaseResponse 
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; } 
    } 
}
