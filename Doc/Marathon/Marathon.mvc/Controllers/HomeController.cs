using System.Diagnostics;
using System.Net.Http.Json;
using Marathon.mvc.Models;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            // ---- 1. Auth gate first, before any data loading ----
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Index", "Account");

            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrWhiteSpace(role))
                return RedirectToAction("Index", "Account");

            // ---- 2. Load dashboard data ----
            var client = _httpClientFactory.CreateClient("ApiClient");
            var model = new DashboardDto();

            try
            {
                var users = await GetListAsync<UserDto>(client, "api/User") ?? new();
                var events = await GetListAsync<EventDto>(client, "api/Events") ?? new();
                var categories = await GetListAsync<CategoryDto>(client, "api/Category") ?? new();
                var enrolments = await GetListAsync<EnrolmentDto>(client, "api/Enrolment") ?? new();
                var results = await GetListAsync<ResultDto>(client, "api/Results") ?? new();
                var payments = await GetListAsync<PaymentDto>(client, "api/Payment") ?? new();

                model.UserCount = users.Count;
                model.EventCount = events.Count;
                model.CategoryCount = categories.Count;
                model.EnrolmentCount = enrolments.Count;
                model.ResultCount = results.Count;

                model.UpcomingEvents = events;
                model.Categories = categories;
                model.RecentEnrolments = enrolments;
                model.RecentResults = results;
                model.Payments = payments;

                model.TotalRevenue = payments
                    .Where(p => string.Equals(p.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
            }

            // ---- 3. Route to the role-specific view ----
            return role switch
            {
                "Admin" => View("Admin", model),
                "Participant" => View("Participant", model),
                "User" => View("User", model),
                _ => Forbid()
            };
        }

        private async Task<List<T>?> GetListAsync<T>(HttpClient client, string path)
        {
            try
            {
                return await client.GetFromJsonAsync<List<T>>(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve data from {Path}", path);
                return null;
            }
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}