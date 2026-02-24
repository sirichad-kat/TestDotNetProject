using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DotnetProject.Core.Features.TestExternal.DTO
{
    public class ProductResponse
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public ProductData? Data { get; set; }
        public long? CreatedAt { get; set; }
    }
}
