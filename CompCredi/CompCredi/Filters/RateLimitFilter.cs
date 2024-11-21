using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace CompCredi.Filters {
    public class RateLimitFilter : IActionFilter {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(60);

        public RateLimitFilter(IMemoryCache cache) {
            _cache = cache;
        }

        public void OnActionExecuting(ActionExecutingContext context) {
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (ipAddress == null) return;

            if (_cache.TryGetValue(ipAddress, out int requestCount)) {
                if (requestCount >= 10) {
                    context.Result = new ContentResult {
                        Content = "Too many requests, please try again later.",
                        StatusCode = 429
                    };
                    return;
                }
                _cache.Set(ipAddress, requestCount + 1, RequestTimeout);
            }
            else {
                _cache.Set(ipAddress, 1, RequestTimeout);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
