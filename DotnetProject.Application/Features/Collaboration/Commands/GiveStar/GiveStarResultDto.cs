using DotnetProject.Application.Features.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    /// <summary>
    /// Represents the result of a successful GiveStar command.
    /// </summary>
    public class GiveStarResultDto : BaseResponse
    {
        public int Id { get; set; }
    }
}
