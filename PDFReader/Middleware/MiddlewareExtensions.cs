using Microsoft.AspNetCore.Builder;

namespace PDFReader.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDeviceAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DeviceAuthMiddleware>();
        }
    }
} 