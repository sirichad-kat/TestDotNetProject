
using FeedCommonLib.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    public record GiveStarCommand(string StarUser, string? StarFullname, int? Sprint, int? Year, string? Remark, string? GivenUser, string? GivenFullname,  string? SubTeam) : ICommandRequest<GiveStarResultDto>;


}
