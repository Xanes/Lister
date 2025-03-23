using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PDFReader.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IDeviceAuthService _deviceAuthService;

        public AuthController(IDeviceAuthService deviceAuthService)
        {
            _deviceAuthService = deviceAuthService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.DeviceMac))
            {
                return BadRequest("Password and device MAC address are required");
            }

            var result = await _deviceAuthService.AuthenticateWithPasswordAsync(
                request.Password, 
                request.DeviceMac);

            if (result)
            {
                return Ok(new { message = "Authentication successful", trusted = true });
            }

            return Unauthorized(new { message = "Invalid password", trusted = false });
        }

        [HttpDelete("device/{macAddress}")]
        public async Task<IActionResult> RemoveDevice(string macAddress)
        {
            // In a real app you would check if the user has permission to remove devices
            var result = await _deviceAuthService.RemoveTrustedDeviceAsync(macAddress);
            
            if (result)
            {
                return Ok(new { message = "Device removed" });
            }
            
            return NotFound(new { message = "Device not found" });
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckDevice([FromHeader(Name = "X-Device-Mac")] string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress))
            {
                return BadRequest("Device MAC address is required");
            }

            var isTrusted = await _deviceAuthService.IsDeviceTrustedAsync(macAddress);
            
            return Ok(new { trusted = isTrusted });
        }
    }

    public class LoginRequest
    {
        public string Password { get; set; }
        public string DeviceMac { get; set; }
    }
} 