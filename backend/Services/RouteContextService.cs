using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OnboardingAssistant.Models;

namespace OnboardingAssistant.Services
{
    public class RouteContextService
    {
        private readonly string _dataDirectory;
        private Dictionary<string, RouteContext> _routeContexts;

        public RouteContextService(IConfiguration configuration)
        {
            _dataDirectory = configuration["Data:Directory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _routeContexts = new Dictionary<string, RouteContext>();
            
            // Load route contexts from JSON files if they exist
            // This is only for backward compatibility with existing JSON files
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

                Console.WriteLine($"Loaded {_routeContexts.Count} route contexts for backward compatibility");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading route contexts: {ex.Message}");
            }
        }

        public RouteContext GetContextForRoute(string route)
        {
            // Normalize the route
            route = NormalizeRoute(route);

            // Check if we have an exact match in our legacy data
            if (_routeContexts.TryGetValue(route, out var exactMatch))
            {
                return exactMatch;
            }

            // If no match found, return a minimal context with just the route
            // OpenAI's Retrieval system will handle finding the relevant context
            return new RouteContext
            {
                Route = route,
                Description = "Using OpenAI Retrieval for context.",
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
    }
}
