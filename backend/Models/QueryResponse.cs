using System.Collections.Generic;

namespace OnboardingAssistant.Models
{
    public class QueryResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<AssistantAction>? Actions { get; set; }
        public string ThreadId { get; set; } = string.Empty;
    }

    public class AssistantAction
    {
        public string Type { get; set; } = string.Empty;
        public string? ElementId { get; set; }
        public string? Description { get; set; }
        public string? Route { get; set; }
    }
}
