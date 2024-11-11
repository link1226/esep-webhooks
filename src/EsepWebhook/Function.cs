using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.Lambda.Core;

public class Function
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<string> FunctionHandler(dynamic input, ILambdaContext context)
    {
        // Get Slack URL from environment variable
        string slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");

        // Parse the GitHub webhook payload
        string issueUrl = input.issue.html_url;
        string issueTitle = input.issue.title;
        string issueBody = input.issue.body;
        string sender = input.sender.login;

        // Construct the Slack message
        var message = new { text = $"Issue created by {sender}: *<{issueUrl}|{issueTitle}>*\n{issueBody}" };
        var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

        // Send message to Slack
        HttpResponseMessage response = await client.PostAsync(slackUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            return "Failed to send message to Slack";
        }
        
        return "Message sent to Slack";
    }
}
