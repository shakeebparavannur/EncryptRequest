using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EncryptRequest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncryptController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("sample data");
        }
    }
}
