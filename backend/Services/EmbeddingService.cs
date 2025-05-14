using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OnboardingAssistant.Models;

namespace OnboardingAssistant.Services
{
    public class EmbeddingService
    {
        private readonly string _apiKey;
        private readonly string _embeddingModel;
        private readonly string _dataDirectory;
        private readonly OpenAIClient _client;
        private Dictionary<string, RouteContext> _routeContexts;

        public EmbeddingService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey configuration is missing");
            _embeddingModel = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-ada-002";
            _dataDirectory = configuration["Data:Directory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _client = new OpenAIClient(_apiKey);
            _routeContexts = new Dictionary<string, RouteContext>();
            
            // Load route contexts from JSON files
            LoadRouteContexts();
        }

        private void LoadRouteContexts()
        {
            try
            {
                if (!Directory.Exists(_dataDirectory))
                {
                    Console.WriteLine($"Data directory not found: {_dataDirectory}");
                    return;
                }

                var jsonFiles = Directory.GetFiles(_dataDirectory, "*.json");
                foreach (var file in jsonFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var context = JsonSerializer.Deserialize<RouteContext>(json);
                        if (context != null && !string.IsNullOrEmpty(context.Route))
                        {
                            _routeContexts[context.Route] = context;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading context from {file}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Loaded {_routeContexts.Count} route contexts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading route contexts: {ex.Message}");
            }
        }

        public async Task<RouteContext> GetContextForRouteAsync(string route)
        {
            // Normalize the route
            route = NormalizeRoute(route);

            // Check if we have an exact match
            if (_routeContexts.TryGetValue(route, out var exactMatch))
            {
                return exactMatch;
            }

            // If no exact match, try to find the most similar route using embeddings
            if (_routeContexts.Count > 0)
            {
                var routeEmbedding = await GetEmbeddingAsync(route);
                var mostSimilarRoute = await FindMostSimilarRouteAsync(routeEmbedding);
                
                if (mostSimilarRoute != null && _routeContexts.TryGetValue(mostSimilarRoute, out var similarMatch))
                {
                    return similarMatch;
                }
            }

            // If no match found, return a default context
            return new RouteContext
            {
                Route = route,
                Description = "No specific information available for this route.",
                Elements = new List<UiElement>(),
                ApiCalls = new List<string>(),
                UserActions = new List<string>()
            };
        }

        private string NormalizeRoute(string route)
        {
            // Remove trailing slash if present
            return route.TrimEnd('/');
        }

        private async Task<float[]> GetEmbeddingAsync(string text)
        {
            try
            {
                var response = await _client.GetEmbeddingsAsync(new EmbeddingsOptions(_embeddingModel, new List<string> { text }));
                return response.Value.Data[0].Embedding.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting embedding: {ex.Message}");
                return Array.Empty<float>();
            }
        }

        private async Task<string> FindMostSimilarRouteAsync(float[] queryEmbedding)
        {
            if (queryEmbedding.Length == 0 || _routeContexts.Count == 0)
            {
                return null;
            }

            string bestMatch = null;
            float highestSimilarity = -1;

            foreach (var route in _routeContexts.Keys)
            {
                var routeEmbedding = await GetEmbeddingAsync(route);
                var similarity = CosineSimilarity(queryEmbedding, routeEmbedding);
                
                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    bestMatch = route;
                }
            }

            // Only return a match if the similarity is above a threshold
            return highestSimilarity > 0.7 ? bestMatch : null;
        }

        private float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length || a.Length == 0)
            {
                return 0;
            }

            float dotProduct = 0;
            float normA = 0;
            float normB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return dotProduct / (float)(Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
