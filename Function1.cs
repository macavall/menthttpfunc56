using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace menthttpfunc56
{
    public class Function1
    {
        private readonly ILogger _logger;
        static HttpClient client = new HttpClient();
        static readonly int LoopCount = Convert.ToInt32(Environment.GetEnvironmentVariable("LoopCount"));
        static readonly int SemaphoreCount = Convert.ToInt32(Environment.GetEnvironmentVariable("SemaphoreCount"));
        static readonly string url = "https://" + Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");


        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            var semaphore = new SemaphoreSlim(SemaphoreCount);

            Task.Factory.StartNew(async () =>
            {
                var tasks = new List<Task>();
                for (int i = 0; i < LoopCount; i++)
                {
                    await semaphore.WaitAsync();

                    var selfReq = new HttpRequestMessage(HttpMethod.Get, url);
    
                    tasks.Add(client.SendAsync(selfReq).ContinueWith((t) => semaphore.Release()));
                }
                await Task.WhenAll(tasks);
            });

            return response;
        }
    }
}
