using System;
using System.Collections.Generic;

namespace DotnetProject.Infrastructure.Entities;

public partial class FwInit
{
    public string ProgramId { get; set; } = null!;

    public string KeyName { get; set; } = null!;

    public string? Descr { get; set; }

    public string? Value { get; set; }
}
