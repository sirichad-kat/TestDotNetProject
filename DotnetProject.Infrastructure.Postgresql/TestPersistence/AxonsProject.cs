using System;
using System.Collections.Generic;

namespace DotnetProject.Infrastructure.Postgresql.TestPersistence;

public partial class AxonsProject
{
    public int Year { get; set; }

    public string ProjectCode { get; set; } = null!;

    public string? ProjectName { get; set; }

    public string? SubTeam { get; set; }

    public DateTime? CreatedAt { get; set; }
}
