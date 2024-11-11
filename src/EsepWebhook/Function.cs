using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// A function that posts an issue creation message to Slack
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(object input, ILambdaContext context)
        {
            context.Logger.LogInformation($"FunctionHandler received: {input}");

            // Deserialize the input JSON safely
            try
            {
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());

                // Ensure the fields exist in the JSON payload
                if (json.issue == null || json.issue.html_url == null)
                {
                    context.Logger.LogInformation("Missing issue or issue URL in payload");
                    return "Error: Missing issue or issue URL in payload";
                }

                string issueUrl = json.issue.html_url;

                // Construct the Slack message payload with double quotes
                string payload = JsonConvert.SerializeObject(new { text = $"Issue Created: {issueUrl}" });

                // Create the HTTP request
                var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                // Send the request asynchronously
                var response = await client.SendAsync(webRequest);
                string responseContent = await response.Content.ReadAsStringAsync();

                context.Logger.LogInformation($"Response from Slack: {responseContent}");

                return responseContent;
            }
            catch (JsonException ex)
            {
                context.Logger.LogError($"Error parsing JSON: {ex.Message}");
                return $"Error parsing JSON: {ex.Message}";
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Unexpected error: {ex.Message}");
                return $"Unexpected error: {ex.Message}";
            }
        }
    }
}
