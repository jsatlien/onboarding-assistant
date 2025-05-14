using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OnboardingAssistant.Models;
using OnboardingAssistant.Services;

namespace OnboardingAssistant.Controllers
{
    [ApiController]
    [Route("api")]
    [EnableCors("AllowAll")]
    public class AssistantController : ControllerBase
    {
        private readonly OpenAIService _openAIService;
        private readonly EmbeddingService _embeddingService;

        public AssistantController(OpenAIService openAIService, EmbeddingService embeddingService)
        {
            _openAIService = openAIService;
            _embeddingService = embeddingService;
        }

        [HttpPost("query")]
        public async Task<ActionResult<QueryResponse>> Query([FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return BadRequest("Query cannot be empty");
            }

            // Get context for the current route
            var context = await _embeddingService.GetContextForRouteAsync(request.Route);

            // Process the query with the OpenAI service, passing the thread ID if available
            var response = await _openAIService.ProcessQueryAsync(request.Query, context, request.ThreadId);

            return Ok(response);
        }

        [HttpGet("context")]
        public async Task<ActionResult<RouteContext>> GetContext([FromQuery] string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                return BadRequest("Route cannot be empty");
            }

            var context = await _embeddingService.GetContextForRouteAsync(route);
            return Ok(context);
        }
    }
}
