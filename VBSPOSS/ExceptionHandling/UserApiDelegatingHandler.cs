using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace VBSPOSS.ExceptionHandling
{
    public class UserApiDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserApiDelegatingHandler> _logger;

        public UserApiDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<UserApiDelegatingHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage httpResponseMessage = null;
            try
            {
                string accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                Console.WriteLine(accessToken);
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception($"Access token is missing for the request {request.RequestUri}");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                httpResponseMessage = await base.SendAsync(request, cancellationToken);
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run http query {RequestUri}", request.RequestUri);
                //throw;
                //return httpResponseMessage;
            }
            return httpResponseMessage;
        }
    }
}
