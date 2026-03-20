using DryMartiniMovies.Infrastructure.Neo4j;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DryMartiniMovies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly Neo4jContext _context;

        public HealthController(Neo4jContext context) 
        {
            _context = context;
        }

        [HttpGet("neo4j")]
        public async Task<IActionResult> CheckNeo4j()
        {
            try
            {
                await _context.VerifyConnectivityAsync();
                return Ok("Neo4j connection OK.");
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Neo4j connection failed: {ex.Message}");
            }
        }
    }
}
