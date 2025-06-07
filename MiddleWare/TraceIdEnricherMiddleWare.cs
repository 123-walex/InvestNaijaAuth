using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;

namespace InvestNaijaAuth.MiddleWare
    {
    public class TraceIdEnricherMiddleWare
    {
        private readonly RequestDelegate _next;

        public TraceIdEnricherMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get the trace identifier for the current request
            var traceId = context.TraceIdentifier;

            // Add it to Serilog's log context, so it can be used in output templates
            using (LogContext.PushProperty("TraceId", traceId))
            {
                await _next(context); // Continue processing the request
            }
        }
    }
}

