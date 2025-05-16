using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OnboardingAssistant.Models;

namespace OnboardingAssistant.Services
{
    public class OpenAIService
    {
        private readonly string _apiKey;
        private readonly string _assistantId;
        private readonly OpenAIClient _client;

        public OpenAIService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey configuration is missing");
            _assistantId = configuration["OpenAI:AssistantId"] ?? throw new ArgumentNullException("OpenAI:AssistantId configuration is missing");
            _client = new OpenAIClient(_apiKey);
        }

        public async Task<QueryResponse> ProcessQueryAsync(string query, RouteContext context, string threadId = null)
        {
            try
            {
                // Create a new thread or use existing one
                string currentThreadId;
                if (string.IsNullOrEmpty(threadId))
                {
                    var threadResponse = await _client.CreateThreadAsync(new CreateThreadOptions());
                    currentThreadId = threadResponse.Value.Id;
                }
                else
                {
                    currentThreadId = threadId;
                }

                // Add a message to the thread with the current route for better context retrieval
                var messageContent = $"The user is currently on {context.Route} and has asked: {query}";
                await _client.CreateMessageAsync(currentThreadId, new CreateMessageOptions
                {
                    Role = ChatRole.User,
                    Content = messageContent
                });

                // Run the assistant on the thread
                var runOptions = new RunThreadOptions
                {
                    AssistantId = _assistantId
                };
                var runResponse = await _client.CreateRunAsync(currentThreadId, runOptions);
                var runId = runResponse.Value.Id;

                // Poll for completion
                bool isCompleted = false;
                Run run = null;
                while (!isCompleted)
                {
                    await Task.Delay(1000); // Wait for 1 second before checking again
                    var getRunResponse = await _client.GetRunAsync(threadId, runId);
                    run = getRunResponse.Value;

                    if (run.Status == RunStatus.Completed || 
                        run.Status == RunStatus.Failed || 
                        run.Status == RunStatus.Cancelled)
                    {
                        isCompleted = true;
                    }
                }

                if (run.Status != RunStatus.Completed)
                {
                    throw new Exception($"Assistant run failed with status: {run.Status}");
                }

                // Get the messages (including the assistant's response)
                var messagesResponse = await _client.GetMessagesAsync(currentThreadId);
                var messages = messagesResponse.Value.Data;

                // Find the assistant's response (should be the most recent message)
                string assistantResponse = string.Empty;
                List<AssistantAction> actions = new List<AssistantAction>();

                foreach (var message in messages)
                {
                    if (message.Role.ToString() == "assistant")
                    {
                        // Get the text content
                        foreach (var content in message.Content)
                        {
                            if (content is MessageTextContent textContent)
                            {
                                assistantResponse = textContent.Text;
                            }
                        }

                        // Parse any actions from the response
                        // This is a simplified example - in a real implementation, 
                        // you might use function calling or a more structured approach
                        if (assistantResponse.Contains("[[highlight:"))
                        {
                            // Extract highlight actions using a simple parsing approach
                            ExtractActions(assistantResponse, actions);
                            
                            // Remove the action markers from the response
                            assistantResponse = CleanResponse(assistantResponse);
                        }

                        break; // We only need the most recent assistant message
                    }
                }

                return new QueryResponse
                {
                    Response = assistantResponse,
                    Actions = actions.Count > 0 ? actions : null,
                    ThreadId = currentThreadId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing query: {ex.Message}");
                return new QueryResponse
                {
                    Response = "I'm sorry, I encountered an error while processing your request."
                };
            }
        }

        private void ExtractActions(string response, List<AssistantAction> actions)
        {
            // This is a simplified example of parsing actions from the response
            // In a real implementation, you might use a more structured approach
            
            // Extract highlight actions
            int highlightStart = response.IndexOf("[[highlight:");
            while (highlightStart >= 0)
            {
                int highlightEnd = response.IndexOf("]]", highlightStart);
                if (highlightEnd > highlightStart)
                {
                    string actionText = response.Substring(highlightStart + 12, highlightEnd - highlightStart - 12);
                    string[] parts = actionText.Split('|');
                    if (parts.Length >= 2)
                    {
                        actions.Add(new AssistantAction
                        {
                            Type = "highlight",
                            ElementId = parts[0].Trim(),
                            Description = parts[1].Trim()
                        });
                    }
                    
                    highlightStart = response.IndexOf("[[highlight:", highlightEnd);
                }
                else
                {
                    break;
                }
            }
            
            // Extract navigate actions
            int navigateStart = response.IndexOf("[[navigate:");
            while (navigateStart >= 0)
            {
                int navigateEnd = response.IndexOf("]]", navigateStart);
                if (navigateEnd > navigateStart)
                {
                    string actionText = response.Substring(navigateStart + 11, navigateEnd - navigateStart - 11);
                    actions.Add(new AssistantAction
                    {
                        Type = "navigate",
                        Route = actionText.Trim()
                    });
                    
                    navigateStart = response.IndexOf("[[navigate:", navigateEnd);
                }
                else
                {
                    break;
                }
            }
        }

        private string CleanResponse(string response)
        {
            // Remove action markers from the response
            response = response.Replace(new string[] { "[[highlight:", "[[navigate:" }, "[[", StringSplitOptions.None);
            response = response.Replace("]]", "");
            return response;
        }
    }
}
