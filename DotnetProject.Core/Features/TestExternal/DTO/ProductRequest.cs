using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DotnetProject.Core.Features.TestExternal
{

    public class ProductDTO
    {
        public string? UrlRequest { get; set; }
        public ProductRequest? RequestData { get; set; }
    }

    public class ProductRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("data")]
        public ProductData? Data { get; set; }
    }

    public class ProductData
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("CPU model")]
        public string? CpuModel { get; set; }

        [JsonPropertyName("Hard disk size")]
        public string? HardDiskSize { get; set; }
    }

}
