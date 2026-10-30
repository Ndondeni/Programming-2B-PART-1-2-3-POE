using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.mvc.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IHttpClientFactory httpFactory, ILogger<ProfileController> logger)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("Email") ?? string.Empty;
            var users = await GetListAsync<UserDto>("api/User") ?? new List<UserDto>();

            var model = users.FirstOrDefault(u =>
                !string.IsNullOrWhiteSpace(email) &&
                string.Equals((u.Username ?? string.Empty), email, StringComparison.OrdinalIgnoreCase));

            return View(model ?? new UserDto());
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
                    _logger.LogWarning("Profile API call failed: {StatusCode} {Path}", response.StatusCode, path);
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
