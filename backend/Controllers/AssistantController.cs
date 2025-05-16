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
        private readonly RouteContextService _routeContextService;

        public AssistantController(OpenAIService openAIService, RouteContextService routeContextService)
        {
            _openAIService = openAIService;
            _routeContextService = routeContextService;
        }

        [HttpPost("query")]
        public async Task<ActionResult<QueryResponse>> Query([FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return BadRequest("Query cannot be empty");
            }

            // Get context for the current route
            var context = _routeContextService.GetContextForRoute(request.Route);

            // Process the query with the OpenAI service, passing the thread ID if available
            var response = await _openAIService.ProcessQueryAsync(request.Query, context, request.ThreadId);

            return Ok(response);
        }

        [HttpGet("context")]
        public ActionResult<RouteContext> GetContext([FromQuery] string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                return BadRequest("Route cannot be empty");
            }

            var context = _routeContextService.GetContextForRoute(route);
            return Ok(context);
        }
    }
}
