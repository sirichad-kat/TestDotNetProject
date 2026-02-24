using IntegrationTestLib;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers; 

namespace DotnetProject.IntegrationTest
{
    public class IntegrationTestBase : IDisposable
    {
        protected static WebApplicationFactory<Program> Factory => TestFixture.Factory;
        protected HttpClient Client;
        protected IConfiguration Configuration => TestFixture.Configuration;
        protected IntegrationFunction fn => TestFixture.Fn;
        protected string jsonBasePath => TestFixture.JsonBasePath;
        protected string scriptBasePath => TestFixture.ScriptBasePath;

        protected IntegrationTestBase()
        {
            Client = Factory.CreateClient();

            //Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fn.Token);
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}
