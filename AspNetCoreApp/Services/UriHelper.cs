using Microsoft.AspNetCore.Http;

namespace AspNetCoreApp.Services
{
    public static class UriHelper
    {
        public static string GetCurrentUri(this HttpRequest request)
        {
            return string.Concat(
                           request.Scheme,
                           "://",
                           request.Host.ToUriComponent(),
                           request.PathBase.ToUriComponent(),
                           request.Path.ToUriComponent(),
                           request.QueryString.ToUriComponent());
        }
    }
}
