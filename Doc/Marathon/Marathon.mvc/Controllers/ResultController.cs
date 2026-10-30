using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.mvc.Controllers
{
    [Authorize]
    public class ResultController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<ResultController> _logger;

        public ResultController(IHttpClientFactory httpFactory, ILogger<ResultController> logger)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var results = await GetListAsync<ResultDto>("api/Results") ?? new List<ResultDto>();

            var role = HttpContext.Session.GetString("Role");
            if (string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
            {
                // The API model does not expose a direct user->result link.
                // Keep the list safe and non-destructive rather than guessing at an invalid join.
            }

            return View(results);
        }

        private async Task<List<T>?> GetListAsync<T>(string path)
        {
            var client = _httpFactory.CreateClient("ApiClient");
            using var request = new HttpRequestMessage(HttpMethod.Get, path);

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                using var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Result API call failed: {StatusCode} {Path}", response.StatusCode, path);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<List<T>>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve data from {Path}", path);
                return null;
            }
        }
    }
}
