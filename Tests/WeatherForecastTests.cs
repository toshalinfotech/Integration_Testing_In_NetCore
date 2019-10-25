using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApi;
using WebApi.Controllers;
using Xunit;

namespace Tests
{
    public class WeatherForecastTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public WeatherForecastTests()
        {
            var projectDir = GetProjectPath("", typeof(WeatherForecastController).GetTypeInfo().Assembly);

            _server = new TestServer(new WebHostBuilder()
                 .UseEnvironment("development")
                  .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(projectDir)
                    .AddJsonFile("appsettings.json")
                    .Build()
                )
                .UseStartup<Startup>());
            _client = _server.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5000");
        }

        private static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            // Get name of the target project which we want to test
            var projectName = startupAssembly.GetName().Name;

            // Get currently executing test project path
            var applicationBasePath = System.AppContext.BaseDirectory;

            // Find the path to the target project
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;

                var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));
                if (projectDirectoryInfo.Exists)
                {
                    var projectFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj"));
                    if (projectFileInfo.Exists)
                    {
                        return Path.Combine(projectDirectoryInfo.FullName, projectName);
                    }
                }
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }

        [Fact]
        public async Task Get()
        {
            var result = await _client.GetAsync("http://localhost:5000/WeatherForecast/get");
            result.Should().NotBeNull();
            var content = result.Content.ReadAsStringAsync().Result;
            content.Should().NotBeNull();
        }


        [Fact]
        public async Task Post()
        {
            var request = new WeatherForecast() { };
            var result = await _client.PostAsync("http://localhost:5000/WeatherForecast/post", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            result.Should().NotBeNull();
            result.Should().NotBeNull();
            var content = result.Content.ReadAsStringAsync().Result;
            content.Should().NotBeNull();
        }
    }
}
