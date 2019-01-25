using System;
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
    public static class AlexaCalculatorFunction
    {
        [FunctionName("AlexaCalculatorFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/calculator")]HttpRequest req, ILogger log)
        {
            log.LogInformation("AlexaCalculatorFunction - Started");

            // Get request body
            var content = await req.ReadAsStringAsync();

            // Deserialize object to SkillRequest
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(content);

            // Validate SkillRequest
            var isValid = await skillRequest.ValidateRequestAsync(req, log);
            if (!isValid)
                return new BadRequestResult();

            // check the request type for Launch Request
            if (skillRequest.Request is LaunchRequest)
            {
                // default launch request
                log.LogInformation("AlexaCalculatorFunction - LaunchRequest");
                return new OkObjectResult(HandleHelpRequest());
            }

            // check the request type for Intent Request
            if (skillRequest.Request is IntentRequest)
            {
                var intent = ((IntentRequest)skillRequest.Request).Intent;

                log.LogInformation($"AlexaCalculatorFunction - IntentRequest: {intent}");

                if (!intent.Slots.ContainsKey("firstnum") || !intent.Slots.ContainsKey("secondnum"))
                    return new OkObjectResult(ResponseBuilder.TellWithCard("Please specify two numbers to be added, subtracted, multiplied or divided.", "Alexa Calculator", "Unfortunately, no numbers were given. Please try again with a math task."));

                var num1 = Convert.ToDouble(intent.Slots["firstnum"].Value);
                var num2 = Convert.ToDouble(intent.Slots["secondnum"].Value);
                double result;

                switch (intent.Name)
                {
                    case "AddIntent":
                        result = num1 + num2;
                        return new OkObjectResult(ResponseBuilder.TellWithCard($"The result of adding {num1} and {num2} is: {result}.", "Alexa Calculator", $"{num1} + {num2} = {result}."));
                    case "SubstractIntent":
                        result = num1 - num2;
                        return new OkObjectResult(ResponseBuilder.TellWithCard($"The result of subtracting {num1} and {num2} is: {result}.", "Alexa Calculator", $"{num1} - {num2} = {result}."));
                    case "MultiplyIntent":
                        result = num1 * num2;
                        return new OkObjectResult(ResponseBuilder.TellWithCard($"The result of multiplying {num1} and {num2} is: {result}.", "Alexa Calculator", $"{num1} * {num2} = {result}."));
                    case "DivideIntent":
                        if (num2 == 0)
                        {
                            return new OkObjectResult(ResponseBuilder.TellWithCard("You have just tried to divide by 0. This does not work. Please try with a different task.", "Alexa Calculator", "You have just tried to divide by 0. This does not work. Please try with a different task."));
                        }
                        else
                        {
                            result = num1 / num2;
                            return new OkObjectResult(ResponseBuilder.TellWithCard($"The result of dividing {num1} and {num2} is: {result:F2}.", "Alexa Calculator", $"{num1} / {num2} = {result:F2}."));
                        }
                    default:
                        return new OkObjectResult(HandleHelpRequest());
                }
            }

            return new OkObjectResult(HandleHelpRequest());
        }

        private static SkillResponse HandleHelpRequest()
        {
            return ResponseBuilder.AskWithCard("Welcome to the Alexa Calculator. I can add, subtract, multiply, and even divide two numbers. For example, what is three plus two?", "Alexa Calculator", "Welcome to the Alexa calculator. I can add, subtract, multiply, and even divide two numbers. For example, what is 3 + 2?", new Reprompt("What is your mathematical task?"));
        }
    }
}
