using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET;
using Alexa.NET.Security.Functions;

namespace AlexaAzureFunctions
{
    public static class AlexaHelloNameFunction
    {
        [FunctionName("AlexaHelloNameFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/helloname")]HttpRequest req, ILogger log)
        {
            log.LogInformation("AlexaHelloNameFunction - Started");

            // Get request body
            var content = await req.ReadAsStringAsync();

            // Deserialize object to SkillRequest
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(content);

            // Validate SkillRequest
            var isValid = await skillRequest.ValidateRequestAsync(req, log);
            if (!isValid)
                return new BadRequestResult();

            // Check for launchRequest
            if (skillRequest.Request is LaunchRequest)
                return new OkObjectResult(ResponseBuilder.AskWithCard("Welcome to Hello Name! Just give me the name of the person I should welcome today.", "Hello Name", "Welcome to Hello Name!", new Reprompt("What is your name?")));

            // get name from body data
            var intentRequest = (IntentRequest)skillRequest.Request;
            var name = intentRequest.Intent.Slots.ContainsKey("name") ? intentRequest.Intent.Slots["name"].Value : null;

            if (name == null)
            {
                log.LogInformation("AlexaHelloNameFunction - No name detected");
                return new OkObjectResult(ResponseBuilder.TellWithCard("Unfortunately, I did not understand your name correctly...", "Hello Name!", "Unfortunately, your name was not recognized..."));
            }

            log.LogInformation($"AlexaHelloNameFunction - Name: {name}");
            return new OkObjectResult(ResponseBuilder.TellWithCard($"How are you, {name.ToUpper()}? I am pleased to meet you.", "Hello Name!", $"Hello {name.ToUpper()}!"));
        }
    }
}
