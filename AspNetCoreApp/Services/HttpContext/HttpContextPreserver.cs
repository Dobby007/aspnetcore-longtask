using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;


namespace AspNetCoreApp.Services.HttpContext
{
    public class HttpContextPreserver : IHttpContextPreserver
    {
        List<System.Timers.Timer> timers = new List<System.Timers.Timer>();
        private readonly IHttpContextAccessor _httpContextAccessor;
        ILogger _logger;

        public HttpContextPreserver(IHttpContextAccessor httpContextAccessor, ILogger<HttpContextPreserver> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void CloneCurrentContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var feature = httpContext.Features.Get<IHttpRequestFeature>();
            feature = new HttpRequestFeature()
            {
                Scheme = feature.Scheme,
                Body = feature.Body,
                Headers = new HeaderDictionary(feature.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)),
                Method = feature.Method,
                Path = feature.Path,
                PathBase = feature.PathBase,
                Protocol = feature.Protocol,
                QueryString = feature.QueryString,
                RawTarget = feature.RawTarget
            };

            var itemsFeature = httpContext.Features.Get<IItemsFeature>();
            itemsFeature = new ItemsFeature()
            {
                Items = itemsFeature?.Items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            var routingFeature = httpContext.Features.Get<IRoutingFeature>();
            routingFeature = new RoutingFeature()
            {
                RouteData = routingFeature.RouteData
            };

            var connectionFeature = httpContext.Features.Get<IHttpConnectionFeature>();
            connectionFeature = new HttpConnectionFeature()
            {
                ConnectionId = connectionFeature?.ConnectionId,
                LocalIpAddress = connectionFeature?.LocalIpAddress,
                RemoteIpAddress = connectionFeature?.RemoteIpAddress,
                
            };

            var collection = new FeatureCollection();
            collection.Set(feature);
            collection.Set(itemsFeature);
            collection.Set(connectionFeature);
            collection.Set(routingFeature);


            var newContext = new DefaultHttpContext(collection);
            _httpContextAccessor.HttpContext = newContext;
        }

        

    }
    public interface IHttpContextPreserver
    {
        void CloneCurrentContext();
    }
    
}
