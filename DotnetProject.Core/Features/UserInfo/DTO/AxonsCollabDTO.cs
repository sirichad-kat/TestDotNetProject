using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Features.UserInfo.DTO
{

    public class AxonsCollabDTO
    {
        public int Id { get; set; }

        public string StarUser { get; set; } = null!;

        public string? StarFullname { get; set; }

        public int? Sprint { get; set; }

        public int? Year { get; set; }

        public string? CategoryCode { get; set; }

        public string? Remark { get; set; }

        public string? GivenUser { get; set; }

        public string? GivenFullname { get; set; }

        public DateTime? GivenDate { get; set; }

        public string? SubTeam { get; set; }
    }
}
