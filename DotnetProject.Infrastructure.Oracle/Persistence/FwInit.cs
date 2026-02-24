using System;
using System.Collections.Generic;

namespace DotnetProject.Infrastructure.Oracle.Persistence;

public partial class FwInit
{
    public string ProgramId { get; set; } = null!;

    public string KeyName { get; set; } = null!;

    public string? Descr { get; set; }

    public string? Value { get; set; }

    public string? Owner { get; set; }

    public DateTime? Accessdate { get; set; }

    public string? Programcode { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedUser { get; set; }

    public string? Function { get; set; }
}
