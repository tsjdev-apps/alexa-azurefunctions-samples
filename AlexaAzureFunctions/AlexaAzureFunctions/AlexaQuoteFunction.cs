using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Security.Functions;

namespace AlexaAzureFunctions
{
    public static class AlexaQuoteFunction
    {
        static class Statics
        {
            public static string QuoteUrl = "https://api.forismatic.com/api/1.0/?method=getQuote&format=json&lang=en";
        }

        class Quote
        {
            [JsonProperty("quoteText")]
            public string QuoteText { get; set; }

            [JsonProperty("quoteAuthor")]
            public string QuoteAuthor { get; set; }
        }

        [FunctionName("AlexaQuoteFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/quote")]HttpRequest req, ILogger log)
        {
            log.LogInformation("AlexaQuoteFunction - Started");

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
                return new OkObjectResult(ResponseBuilder.AskWithCard("Welcome to Random Quote! Everytime you start me and ask for a random quote, I will give it to you..", "Random Quote", "Welcome to Random Quote! Everytime you start me and ask for a random quote, I will give it to you...", new Reprompt("Give me a random quote")));

            // Check for IntentRequest
            if (skillRequest.Request is IntentRequest intentRequest)
            {
                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.StopIntent":
                    case "AMAZON.CancelIntent":
                        return new OkObjectResult(ResponseBuilder.TellWithCard("Ok", "Random Quote", "Till next time."));
                    case "AMAZON.HelpIntent":
                        return new OkObjectResult(ResponseBuilder.AskWithCard("Everytime you ask for a random quote, I will tell you one.", "Random Quote", "Everytime you ask for a random quote, I will tell you one.", new Reprompt("Give me a random quote")));
                    case "RandomQuoteIntent":
                        var quoteString = await new HttpClient().GetStringAsync(Statics.QuoteUrl);
                        var quote = JsonConvert.DeserializeObject<Quote>(quoteString);
                        return new OkObjectResult(ResponseBuilder.TellWithCard($"{quote?.QuoteText?.Trim()} - {quote?.QuoteAuthor}", "Random Quote", $"{quote?.QuoteText?.Trim()} - {quote?.QuoteAuthor}"));
                }
            }

            return new OkObjectResult(ResponseBuilder.TellWithCard("Something went wrong... Please try again.", "Random Quote", "Something went wrong..."));
        }
    }
}
