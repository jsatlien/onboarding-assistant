using System.Collections.Generic;

namespace OnboardingAssistant.Models
{
    public class RouteContext
    {
        public string Route { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<UiElement> Elements { get; set; } = new List<UiElement>();
        public List<string> ApiCalls { get; set; } = new List<string>();
        public List<string>? Dependencies { get; set; }
        public List<string> UserActions { get; set; } = new List<string>();
    }

    public class UiElement
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
