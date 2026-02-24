using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.Features.UserInfo.DTO;
using IntegrationTestLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DotnetProject.IntegrationTest.UserInfo.GetUserProject
{
    [TestFixture]
    public class GetUserProjectTest : IntegrationTestBase
    {
        private static PostgresqlTestDataHelper _testDataHelper; 
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            
                if (_testDataHelper == null)
                {
                    var configuration = Factory.Services.GetRequiredService<IConfiguration>();
                    _testDataHelper = new PostgresqlTestDataHelper(configuration, "DefaultConnection", scriptBasePath);
                } 
        }

        [SetUp]
        public async Task Setup()
        { 
            // เตรียมข้อมูลก่อนแต่ละ test case
            await _testDataHelper.SetupTestData("UserInfo\\GetUserProject\\Setup\\SetupUserProject.sql"); 

        }

        [TearDown]
        public async Task TearDown()
        { 
            await _testDataHelper.CleanupTestData("UserInfo\\GetUserProject\\TearDown\\TeardownUserProject.sql");
        }

        [Test]
        public async Task GetUserProjectSuccess_Should_GetData()
        {

            try
            {
                string _filePath = jsonBasePath + "UserInfo/GetUserProject/GetUserProjectSuccess_Should_GetData.json";
                // Use IEnumerable<ProjectRecord> as TT because ResponseDt already mirrors the ApiResponse structure
                JsonModelTest<GetUserProjectQuery, IEnumerable<ProjectRecord>> jData = fn.GetJsonObject<GetUserProjectQuery, IEnumerable<ProjectRecord>>(_filePath);

                if (jData != null)
                {
                    // Act  
                    string url = $"/api/userinfo/project?{jData.QueryString}";
                    HttpResponseMessage actualResponse = await Client.GetAsync(url);
                     
                    // Compare status and data against expected values from JSON
                    CompareResult compareResult = await fn.CompareResponseByObject<IEnumerable<ProjectRecord>>(actualResponse, jData.WithResponse);
                    Assert.That(compareResult.IsEqual, Is.True, compareResult.Message);
                }
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"JSON Reader Exception: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                throw;
            } 
        }

        [Test]
        public async Task GetUserProjectUserIsEmpty_Should_ReturnBadRequest()
        {

            try
            {
                string _filePath = jsonBasePath + "UserInfo/GetUserProject/GetUserProjectUserIsEmpty_Should_ReturnBadRequest.json";
                // Use IEnumerable<ProjectRecord> as TT because ResponseDt already mirrors the ApiResponse structure
                JsonModelTest<GetUserProjectQuery, IEnumerable<ProjectRecord>> jData = fn.GetJsonObject<GetUserProjectQuery, IEnumerable<ProjectRecord>>(_filePath);

                if (jData != null)
                {
                    // Act  
                    string url = $"/api/userinfo/project?{jData.QueryString}";
                    HttpResponseMessage actualResponse = await Client.GetAsync(url);

                    // Compare status and data against expected values from JSON
                    CompareResult compareResult = await fn.CompareResponseByObject<IEnumerable<ProjectRecord>>(actualResponse, jData.WithResponse);
                    Assert.That(compareResult.IsEqual, Is.True, compareResult.Message);
                }
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"JSON Reader Exception: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                throw;
            }
        }
    }
}
