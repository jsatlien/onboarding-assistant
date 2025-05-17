using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AssistantController> _logger;

        public AssistantController(
            OpenAIService openAIService, 
            RouteContextService routeContextService,
            ILogger<AssistantController> logger)
        {
            _openAIService = openAIService;
            _routeContextService = routeContextService;
            _logger = logger;
            
            _logger.LogInformation("AssistantController initialized");
        }

        [HttpPost("query")]
        public async Task<ActionResult<QueryResponse>> Query([FromBody] QueryRequest request)
        {
            _logger.LogInformation(
                "Received query request: '{Query}' for route '{Route}' with threadId '{ThreadId}'", 
                request.Query, 
                request.Route, 
                request.ThreadId ?? "<null>");
                
            if (string.IsNullOrEmpty(request.Query))
            {
                _logger.LogWarning("Rejected empty query request");
                return BadRequest("Query cannot be empty");
            }

            // Get context for the current route
            var context = _routeContextService.GetContextForRoute(request.Route);
            _logger.LogInformation("Retrieved context for route '{Route}'", request.Route);

            // Process the query with the OpenAI service, passing the thread ID if available
            _logger.LogInformation("Processing query with OpenAI service");
            var response = await _openAIService.ProcessQueryAsync(request.Query, context, request.ThreadId);
            _logger.LogInformation(
                "Query processed successfully. Response threadId: '{ThreadId}', message length: {MessageLength}", 
                response.ThreadId, 
                response.Message?.Length ?? 0);

            return Ok(response);
        }

        [HttpGet("context")]
        public ActionResult<OnboardingAssistant.Models.RouteContext> GetContext([FromQuery] string route)
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
