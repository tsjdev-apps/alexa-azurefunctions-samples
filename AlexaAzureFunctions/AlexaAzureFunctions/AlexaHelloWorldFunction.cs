using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET;
using Alexa.NET.Security.Functions;

namespace AlexaAzureFunctions
{
    public static class AlexaHelloWorldFunction
    {
        [FunctionName("AlexaHelloWorldFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/helloworld")]HttpRequest req, ILogger log)
        {
            log.LogInformation("AlexaHelloWorldFunction - Started");

            // Get request body
            var content = await req.ReadAsStringAsync();

            // Deserialize object to SkillRequest
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(content);

            // Validate SkillRequest
            var isValid = await skillRequest.ValidateRequestAsync(req, log);
            if (!isValid)
                return new BadRequestResult();

            // Return SkillResponse
            return new OkObjectResult(ResponseBuilder.TellWithCard("Hello World from an Azure Function!", "Hello World", "Hello World from an Azure Function!"));
        }
    }
}
