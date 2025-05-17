using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnboardingAssistant.Models;

namespace OnboardingAssistant.Services
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string AssistantId { get; set; } = string.Empty;
        public bool VerboseLogging { get; set; } = false;
    }

    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _assistantId;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly bool _verboseLogging;

        public OpenAIService(IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
        {
            _logger = logger;
            _assistantId = settings.Value.AssistantId;
            _verboseLogging = settings.Value.VerboseLogging;
            
            _logger.LogInformation("Initializing OpenAI service with Assistant ID: {AssistantId}", _assistantId);
            _logger.LogInformation("Verbose logging is {VerboseLoggingStatus}", _verboseLogging ? "enabled" : "disabled");
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", settings.Value.ApiKey);
                
            // Add the required OpenAI-Beta header for the Assistants API
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<QueryResponse> ProcessQueryAsync(string query, OnboardingAssistant.Models.RouteContext context, string? threadId = null)
        {
            try
            {
                _logger.LogInformation("Processing query: '{Query}' for route: '{Route}' with threadId: '{ThreadId}'", 
                    query, context?.Route ?? "unknown", threadId ?? "<null>");
                
                // Create a new thread if one doesn't exist
                string currentThreadId;
                
                if (string.IsNullOrEmpty(threadId))
                {
                    // Create a new thread
                    currentThreadId = await CreateThreadAsync();
                }
                else if (!threadId.StartsWith("thread_"))
                {
                    // For v2 API, thread IDs must start with 'thread_'
                    // If we have an old format thread ID, create a new one
                    _logger.LogWarning("Thread ID {ThreadId} is not in v2 format. Creating a new thread.", threadId);
                    currentThreadId = await CreateThreadAsync();
                }
                else
                {
                    // Use the existing thread ID
                    currentThreadId = threadId;
                }
                
                _logger.LogInformation("Using thread ID: {ThreadId}", currentThreadId);
                
                // Add the user's message to the thread
                await AddMessageToThreadAsync(currentThreadId, query);
                _logger.LogInformation("Added user message to thread");
                
                // Create a run to process the thread with our assistant
                var runId = await CreateRunAsync(currentThreadId, _assistantId);
                _logger.LogInformation("Created run with ID: {RunId}", runId);
                
                // Wait for the run to complete
                var runStatus = await WaitForRunCompletionAsync(currentThreadId, runId);
                _logger.LogInformation("Run completed with status: {Status}", runStatus);
                
                if (runStatus != "completed")
                {
                    _logger.LogWarning("Run did not complete successfully. Status: {Status}", runStatus);
                    
                    string errorMessage;
                    
                    // Check if there's a specific error code from the run
                    bool isRateLimitError = false;
                    
                    try
                    {
                        // Get the last run status to check for specific error codes
                        string requestUrl = $"threads/{currentThreadId}/runs/{runId}";
                        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                        requestMessage.Headers.Authorization = _httpClient.DefaultRequestHeaders.Authorization;
                        requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                        
                        var response = await _httpClient.SendAsync(requestMessage);
                        var responseContent = await response.Content.ReadAsStringAsync();
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                            
                            if (responseObj.TryGetProperty("last_error", out var lastErrorElement) && 
                                !lastErrorElement.ValueKind.Equals(JsonValueKind.Null))
                            {
                                if (lastErrorElement.TryGetProperty("code", out var codeElement) && 
                                    codeElement.GetString() == "rate_limit_exceeded")
                                {
                                    isRateLimitError = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error checking for specific run error codes");
                    }
                    
                    if (isRateLimitError)
                    {
                        errorMessage = "The OpenAI API quota has been exceeded. Please check your billing details in the OpenAI dashboard or contact your administrator.";
                    }
                    else if (runStatus == "failed")
                    {
                        errorMessage = "The assistant encountered an error while processing your request. " +
                                      "This might be because the assistant doesn't have access to the necessary files. " +
                                      "Please check your assistant configuration in the OpenAI dashboard and ensure it has the required files uploaded.";
                    }
                    else if (runStatus == "timeout")
                    {
                        errorMessage = "The assistant took too long to respond. Please try again later.";
                    }
                    else
                    {
                        errorMessage = "I'm sorry, but I couldn't process your request at this time. Please try again later.";
                    }
                    
                    return new QueryResponse
                    {
                        Message = errorMessage,
                        ThreadId = currentThreadId,
                        Actions = new List<AssistantAction>()
                    };
                }
                
                // Get the assistant's response
                var assistantMessage = await GetLatestAssistantMessageAsync(currentThreadId);
                _logger.LogInformation("Retrieved assistant response of length {Length}", assistantMessage.Length);
                
                return new QueryResponse
                {
                    Message = assistantMessage,
                    ThreadId = currentThreadId,
                    Actions = new List<AssistantAction>() // No actions for now
                };
            }
            catch (Exception ex)
            {
                // Log the detailed error for server-side debugging
                _logger.LogError(ex, "Error processing query: {ErrorMessage}", ex.Message);
                
                // Return a generic error message to the user, never exposing technical details
                return new QueryResponse
                {
                    Message = "I'm sorry, but I encountered an error processing your request. Our team has been notified and is working to resolve the issue. Please try again later.",
                    ThreadId = threadId ?? "",
                    Actions = new List<AssistantAction>()
                };
            }
        }
        
        private async Task<string> CreateThreadAsync()
        {
            try
            {
                _logger.LogInformation("Creating new thread");
                var requestContent = new StringContent("{}", Encoding.UTF8, "application/json");
                
                string requestUrl = "threads";
                
                // Ensure the OpenAI-Beta header is set correctly for each request
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                requestMessage.Content = requestContent;
                requestMessage.Headers.Authorization = _httpClient.DefaultRequestHeaders.Authorization;
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                
                // Log the request details
                _logger.LogInformation("Thread creation request URL: {Url}, Method: POST, Headers: {Headers}", 
                    _httpClient.BaseAddress + requestUrl,
                    string.Join(", ", requestMessage.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()));
                
                var response = await _httpClient.SendAsync(requestMessage);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the response details only once
                _logger.LogInformation("Thread creation response Status: {Status}, Headers: {Headers}, Content: {Content}", 
                    (int)response.StatusCode + " " + response.StatusCode,
                    string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()),
                    responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Don't log the response content again since we already logged it above
                    throw new Exception($"Failed to create thread: {response.StatusCode}");
                }
                
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var threadId = responseObj.GetProperty("id").GetString() ?? throw new Exception("Failed to get thread ID");
                
                // Verify that the thread ID starts with 'thread_' as required by v2 API
                if (!threadId.StartsWith("thread_"))
                {
                    _logger.LogWarning("Created thread ID {ThreadId} does not start with 'thread_' prefix required by v2 API", threadId);
                }
                
                _logger.LogInformation("Successfully created thread with ID: {ThreadId}", threadId);
                return threadId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating thread");
                throw;
            }
        }
        
        private async Task AddMessageToThreadAsync(string threadId, string content)
        {
            try
            {
                // The correct format for the message content
                var requestObj = new { role = "user", content = content };
                var requestJson = JsonSerializer.Serialize(requestObj, _jsonOptions);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                string requestUrl = $"threads/{threadId}/messages";
                
                // Ensure the OpenAI-Beta header is set correctly for each request
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                requestMessage.Content = requestContent;
                requestMessage.Headers.Authorization = _httpClient.DefaultRequestHeaders.Authorization;
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                
                // Log the request details
                _logger.LogInformation("Add message request URL: {Url}, Method: POST, Headers: {Headers}, Content: {Content}", 
                    _httpClient.BaseAddress + requestUrl,
                    string.Join(", ", requestMessage.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()),
                    requestJson);
                
                var response = await _httpClient.SendAsync(requestMessage);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the response details only once
                _logger.LogInformation("Add message response Status: {Status}, Headers: {Headers}, Content: {Content}", 
                    (int)response.StatusCode + " " + response.StatusCode,
                    string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()),
                    responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Don't log the response content again since we already logged it above
                    throw new Exception($"Failed to add message to thread: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message to thread");
                throw;
            }
        }
        
        private async Task<string> CreateRunAsync(string threadId, string assistantId)
        {
            try
            {
                var requestObj = new { assistant_id = assistantId };
                var requestJson = JsonSerializer.Serialize(requestObj, _jsonOptions);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                string requestUrl = $"threads/{threadId}/runs";
                
                // Ensure the OpenAI-Beta header is set correctly for each request
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                requestMessage.Content = requestContent;
                requestMessage.Headers.Authorization = _httpClient.DefaultRequestHeaders.Authorization;
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                
                // Log the request details
                _logger.LogInformation("Create run request URL: {Url}, Method: POST, Headers: {Headers}, Content: {Content}", 
                    _httpClient.BaseAddress + requestUrl,
                    string.Join(", ", requestMessage.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()),
                    requestJson);
                
                var response = await _httpClient.SendAsync(requestMessage);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the response details only once
                _logger.LogInformation("Create run response Status: {Status}, Headers: {Headers}, Content: {Content}", 
                    (int)response.StatusCode + " " + response.StatusCode,
                    string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()),
                    responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Don't log the response content again since we already logged it above
                    throw new Exception($"Failed to create run: {response.StatusCode}");
                }
                
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var runId = responseObj.GetProperty("id").GetString() ?? throw new Exception("Failed to get run ID");
                
                _logger.LogInformation("Successfully created run with ID: {RunId}", runId);
                return runId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating run");
                throw;
            }
        }
        
        private async Task<string> WaitForRunCompletionAsync(string threadId, string runId)
        {
            try
            {
                string status = "queued"; // Initialize with a default value
                int attempts = 0;
                const int maxAttempts = 30; // Maximum number of polling attempts (30 seconds)
                
                _logger.LogInformation("Waiting for run {RunId} to complete", runId);
                
                do
                {
                    attempts++;
                    await Task.Delay(1000); // Wait a second between polls
                    
                    if (attempts % 5 == 1) // Only log every 5 attempts to reduce log volume
                    {
                        _logger.LogDebug("Polling run status (attempt {Attempt}/{MaxAttempts})", attempts, maxAttempts);
                    }
                    
                    string requestUrl = $"threads/{threadId}/runs/{runId}";
                    
                    // Ensure the OpenAI-Beta header is set correctly for each request
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    requestMessage.Headers.Authorization = _httpClient.DefaultRequestHeaders.Authorization;
                    requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                    
                    // Only log the full request details on the first attempt or if there's an error
                    if (attempts == 1)
                    {
                        _logger.LogInformation("Get run status request URL: {Url}, Method: GET, Headers: {Headers}", 
                            _httpClient.BaseAddress + requestUrl,
                            string.Join(", ", requestMessage.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()));
                    }
                    
                    var response = await _httpClient.SendAsync(requestMessage);
                    
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    // Only log the full response on the first attempt, last attempt, or if there's an error
                    if (attempts == 1 || !response.IsSuccessStatusCode || status == "completed" || status == "failed")
                    {
                        _logger.LogInformation("Get run status response Status: {Status}, Headers: {Headers}, Content: {Content}", 
                            (int)response.StatusCode + " " + response.StatusCode,
                            string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()),
                            responseContent);
                    }
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        // Don't log the response content again since we already logged it above
                        throw new Exception($"Failed to get run status: {response.StatusCode}");
                    }
                    
                    var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    status = responseObj.GetProperty("status").GetString() ?? "unknown";
                    
                    // Only log the status every 5 attempts to reduce log volume
                    if (attempts % 5 == 1 || status != "queued" && status != "in_progress")
                    {
                        _logger.LogInformation("Run {RunId} status: {Status} (attempt {Attempt})", runId, status, attempts);
                        
                        // If the run failed, get more details about the failure
                        if (status == "failed" && responseObj.TryGetProperty("last_error", out var lastErrorElement) && 
                            !lastErrorElement.ValueKind.Equals(JsonValueKind.Null))
                        {
                            var errorCode = lastErrorElement.TryGetProperty("code", out var codeElement) ? 
                                codeElement.GetString() : "unknown";
                                
                            var errorMessage = lastErrorElement.TryGetProperty("message", out var messageElement) ? 
                                messageElement.GetString() : "Unknown error";
                                
                            _logger.LogError("Run failed with error code: {ErrorCode}, message: {ErrorMessage}", 
                                errorCode, errorMessage);
                        }
                    }
                    
                    // If we've reached the maximum number of attempts, break out of the loop
                    if (attempts >= maxAttempts && (status == "queued" || status == "in_progress"))
                    {
                        _logger.LogWarning("Maximum polling attempts reached. Last status: {Status}", status);
                        return "timeout";
                    }
                    
                } while (status == "queued" || status == "in_progress");
                
                _logger.LogInformation("Run {RunId} completed with status: {Status}", runId, status);
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for run completion");
                throw;
            }
        }
        
        private async Task<string> GetLatestAssistantMessageAsync(string threadId)
        {
            try
            {
                _logger.LogInformation("Getting latest assistant message for thread {ThreadId}", threadId);
                
                string requestUrl = $"threads/{threadId}/messages?limit=10";
                
                // Ensure the OpenAI-Beta header is set correctly for each request
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                requestMessage.Headers.Authorization = _httpClient.DefaultRequestHeaders.Authorization;
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                
                // Log the request details
                _logger.LogInformation("Get messages request URL: {Url}, Method: GET, Headers: {Headers}", 
                    _httpClient.BaseAddress + requestUrl,
                    string.Join(", ", requestMessage.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()));
                
                var response = await _httpClient.SendAsync(requestMessage);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the response details only once
                _logger.LogInformation("Get messages response Status: {Status}, Headers: {Headers}, Content: {Content}", 
                    (int)response.StatusCode + " " + response.StatusCode,
                    string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").ToArray()),
                    responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Don't log the response content again since we already logged it above
                    return "I'm sorry, but I encountered an error processing your request. Our team has been notified and is working to resolve the issue. Please try again later.";
                }
                
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var messages = responseObj.GetProperty("data").EnumerateArray();
                
                _logger.LogDebug("Found {Count} messages in thread", messages.Count());
                
                foreach (var message in messages)
                {
                    var role = message.GetProperty("role").GetString();
                    _logger.LogDebug("Checking message with role: {Role}", role);
                    
                    if (role == "assistant")
                    {
                        _logger.LogDebug("Found assistant message");
                        
                        var content = message.GetProperty("content").EnumerateArray().FirstOrDefault();
                        
                        if (content.TryGetProperty("text", out var textElement) && 
                            textElement.TryGetProperty("value", out var valueElement))
                        {
                            var assistantMessage = valueElement.GetString() ?? "";
                            _logger.LogInformation("Successfully retrieved assistant message of length {Length}", assistantMessage.Length);
                            return assistantMessage;
                        }
                    }
                }
                
                _logger.LogWarning("No assistant messages found in thread {ThreadId}", threadId);
                return "I'm sorry, but I couldn't generate a response at this time.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assistant message");
                return "I'm sorry, but I encountered an error processing your request. Our team has been notified and is working to resolve the issue. Please try again later.";
            }
        }
    }
}