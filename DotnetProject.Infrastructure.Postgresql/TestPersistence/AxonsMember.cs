using System;
using System.Collections.Generic;

namespace DotnetProject.Infrastructure.Postgresql.TestPersistence;

public partial class AxonsMember
{
    public long MemberId { get; set; }

    public string? Username { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? Department { get; set; }

    public string? SquadName { get; set; }

    public char? Status { get; set; }

    public char? NoQa { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
