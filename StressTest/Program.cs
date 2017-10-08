
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace BenchmarkTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Multithread();


            Console.Read();
        }

        static void Multithread()
        {
            try
            {
                using (var test = new Test(true))
                {
                    var tasks = new List<Task>();
                    for (var i = 0; i < 5000; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(async () => await test.MakeRequest(), TaskCreationOptions.LongRunning).Unwrap());
                    }
                    Task.WaitAll(tasks.ToArray());
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} tasks were executed successfully", tasks.Count);
                }
            }
            catch (Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exc.InnerException.ToString());
            }
        }
        
    }
    

    public class Test : IDisposable
    {
        private readonly TestFixture<AspNetCoreApp.Startup> _fixture;
        private readonly Random _random = new Random();

        public Test(bool fixHttpContext)
        {
            _fixture = new TestFixture<AspNetCoreApp.Startup>(fixHttpContext);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        public async Task MakeRequest()
        {
            var _client = _fixture.GetClient();
            var result = await _client.GetAsync($"/Test/{_random.Next()}");
            var json = await result.Content.ReadAsStringAsync();

            var id = Guid.Parse(JsonConvert.DeserializeObject<string>(json));

            while (true)
            {
                var taskResult = await _client.GetAsync($"/GetResult/{id}");
                switch (taskResult.StatusCode)
                {
                    case HttpStatusCode.NoContent:
                        continue;
                    case HttpStatusCode.OK:
                        {
                            var taskContent = await taskResult.Content.ReadAsStringAsync();
                            Console.WriteLine($"Task result is: {taskContent}");
                            return;
                        }

                    default:
                        {
                            var taskContent = await taskResult.Content.ReadAsStringAsync();
                            throw new Exception($"Response is not ok ({(int)taskResult.StatusCode}). Response was: {taskContent}");
                        }
                }
            }
        }
    }
    public class TestFixture<TStartup> : IDisposable
    {
        const string SolutionName = "AspNetCoreApp.sln";
        const string BaseAddress = "http://localhost:5555/";
        readonly IWebHost _host;

        public TestFixture(bool fixHttpContext) : this(fixHttpContext, string.Empty)
        {
        }

        protected TestFixture(bool fixHttpContext, string solutionRelativeTargetProjectParentDir)
        {
            var startupAssembly = typeof(TStartup).Assembly;
            var contentRoot = GetProjectPath(solutionRelativeTargetProjectParentDir, startupAssembly);
            Console.WriteLine($"Content root: {contentRoot}");
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(contentRoot)
                .ConfigureServices(InitializeServices)
                .UseEnvironment(fixHttpContext ? "Good" : "Bad")
                .UseStartup(typeof(TStartup))
                .UseUrls(BaseAddress);

            _host = builder.Build();
            _host.Start();
            

        }
        

        public HttpClient GetClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                UseCookies = true
            })
            {
                BaseAddress = new Uri(BaseAddress),
            };
            return client;
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));

            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());

            services.AddSingleton(manager);
        }

        /// <summary>
        /// Gets the full path to the target project path that we wish to test
        /// </summary>
        /// <param name="solutionRelativePath">
        /// The parent directory of the target project.
        /// e.g. src, samples, test, or test/Websites
        /// </param>
        /// <param name="startupAssembly">The target project's assembly.</param>
        /// <returns>The full path to the target project.</returns>
        private static string GetProjectPath(string solutionRelativePath, Assembly startupAssembly)
        {
            // Get name of the target project which we want to test
            var projectName = startupAssembly.GetName().Name;

            // Get currently executing test project path
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;

            // Find the folder which contains the solution file. We then use this information to find the target
            // project which we want to test.
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var solutionFileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, SolutionName));
                if (solutionFileInfo.Exists)
                {
                    return Path.GetFullPath(Path.Combine(directoryInfo.FullName, solutionRelativePath, projectName));
                }

                directoryInfo = directoryInfo.Parent;
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Solution root could not be located using application root {applicationBasePath}.");
        }
    }
}
