namespace OnboardingAssistant.Models
{
    public class QueryRequest
    {
        public string Query { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string? ThreadId { get; set; } = null;
    }
}
