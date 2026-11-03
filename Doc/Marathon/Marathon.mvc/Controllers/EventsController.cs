using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
//using Marathon.Models.DTOs;

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
        // GET: Events/Create
        [Authorize(Roles = "Admin,Organiser")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [Authorize(Roles = "Admin,Organiser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpFactory.CreateClient("ApiClient");

            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var now = DateTime.UtcNow;

                var eventData = new
                {
                    eventName = model.EventName,
                    eventDate = model.EventDate,
                    now = now,
                    location = model.Location,
                    venue = model.Venue,
                    distanceKm = model.DistanceKm,
                    eventType = model.EventType,
                    eventFee = model.EventFee,
                    status = model.Status,
                    createdBy = 0, 
                    createdAt = now,
                    updatedAt = now
                };

                var response = await client.PostAsJsonAsync(
                    "api/Events",
                    eventData
                );

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Event created successfully!";
                    return RedirectToAction("Index", "Home");
                }

                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", error);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create event.");

                ModelState.AddModelError("", "Could not connect to the API.");

                return View(model);
            }
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