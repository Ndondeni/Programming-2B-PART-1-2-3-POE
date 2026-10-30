using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.mvc.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IHttpClientFactory httpFactory, ILogger<EventsController> logger)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        // ---------- READ ----------
        public async Task<IActionResult> Index()
        {
            var model = await GetListAsync<EventDto>("api/Events") ?? new List<EventDto>();
            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var model = await GetListAsync<EventDto>("api/Events") ?? new List<EventDto>();
            var item = model.FirstOrDefault(e => e.EventId == id);

            if (item is null)
                return NotFound();

            return View(item);
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
                    _logger.LogWarning("Event API call failed: {StatusCode} {Path}", response.StatusCode, path);
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