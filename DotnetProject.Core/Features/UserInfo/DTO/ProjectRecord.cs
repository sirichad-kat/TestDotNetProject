using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Features.UserInfo.DTO
{
    public record ProjectRecord
    {
        public int Year { get; set; }
        public string ProjectCode { get; set; } = null!;
        public string? ProjectName { get; set; }
        public string? SubTeam { get; set; }
        public long MemberId { get; set; }
        public string? Username { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public string? SquadName { get; set; }
        public char? Status { get; set; }
    }
}
