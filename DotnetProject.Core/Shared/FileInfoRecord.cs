using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Shared
{
    public record FileInfoRecord
    {
        public string? ContentType { get; set; }
        public string? FileName { get; set; }
        public Stream? FileStream { get; set; }
        public long? ContentLength { get; set; }
    }
}
