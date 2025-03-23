using Domain.Interfaces;

namespace PDFReader.Middleware
{
    public class DeviceAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string DEVICE_MAC_HEADER = "X-Device-Mac";

        // Define paths that should not require authentication
        private static readonly string[] _allowedPaths = new[]
        {
            "/auth",       // For authentication endpoints
            "/swagger",    // For Swagger UI
            "/api-docs",   // For Swagger docs
            "/health"      // For health checks
        };

        public DeviceAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDeviceAuthService deviceAuthService)
        {
            // Skip authentication for allowed paths
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && IsAllowedPath(path))
            {
                await _next(context);
                return;
            }

            // Get the device MAC address from headers
            if (!context.Request.Headers.TryGetValue(DEVICE_MAC_HEADER, out var macValues))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Device identification required");
                return;
            }

            var macAddress = macValues.ToString();

            // Check if the device is trusted
            if (await deviceAuthService.IsDeviceTrustedAsync(macAddress))
            {
                await _next(context);
                return;
            }

            // Device not trusted
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Device not authorized");
        }

        private bool IsAllowedPath(string path)
        {
            foreach (var allowedPath in _allowedPaths)
            {
                if (path.StartsWith(allowedPath.ToLower()))
                {
                    return true;
                }
            }

            // Allow root path for health checks
            if (path == "/")
            {
                return true;
            }

            return false;
        }
    }
}