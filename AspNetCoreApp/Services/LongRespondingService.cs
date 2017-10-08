using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreApp.Services
{
    public class LongRespondingService : ILongRespondingService
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LongRespondingService(ILogger<LongRespondingService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> DoJob(string requestUri)
        {
            _logger.LogTrace($"DoJob method is starting");

            await Task.Delay(3000);
            var currentUri = _httpContextAccessor.HttpContext.Request.GetCurrentUri();
            var equalness = currentUri == requestUri;
            if (!equalness)
            {
                _logger.LogError($"Something went wrong");
                throw new Exception($"Values does not match: {currentUri} and {requestUri}");
            }
            _logger.LogTrace($"DoJob method is ending");

            return $"Comparison is ok. Values were: {currentUri} and {requestUri}";
            
        }
    }
}
