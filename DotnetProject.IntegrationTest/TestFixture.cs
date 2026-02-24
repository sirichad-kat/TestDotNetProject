using IntegrationTestLib;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.IntegrationTest
{
    [SetUpFixture]
    public class TestFixture
    {
        public static WebApplicationFactory<Program> Factory { get; private set; }
        public static IConfiguration Configuration { get; private set; }
        public static IntegrationFunction Fn { get; private set; }
        public static string JsonBasePath { get; private set; }
        public static string ScriptBasePath { get; private set; }
        //protected HttpClient Client;

        [OneTimeSetUp]
        public void AssemblyInitialize()
        {
            // สร้าง resources ที่ใช้ร่วมกัน
            Factory = new WebApplicationFactory<Program>();

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = configurationBuilder.Build();

            Fn = new IntegrationFunction(Configuration);
            //Fn.GenerateJwtToken("adminfeed.boo", "UAT");
            JsonBasePath = @"D:\Project\DotnetProject\src\DotnetProject.IntegrationTest\jsonResponse\";
            ScriptBasePath = @"D:\Project\DotnetProject\src\DotnetProject.IntegrationTest\Scripts\";

        }

        [OneTimeTearDown]
        public void AssemblyCleanup()
        {
            Factory?.Dispose();
        }
    }
}
