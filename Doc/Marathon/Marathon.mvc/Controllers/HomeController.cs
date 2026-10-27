using Marathon.mvc.Models;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Marathon.mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(
            ILogger<HomeController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var model = new DashboardDto();

            try
            {
                // Dashboard counts
                model.UserCount = await CountAsync(client, "api/User");
                model.EventCount = await CountAsync(client, "api/Event");
                model.CategoryCount = await CountAsync(client, "api/Category");
                model.EnrolmentCount = await CountAsync(client, "api/Enrolment");
                model.ResultCount = await CountAsync(client, "api/Result");

                // Dashboard lists
                model.UpcomingEvents =
                    await GetListAsync<EventDto>(client, "api/Event")
                    ?? new List<EventDto>();

                model.Categories =
                    await GetListAsync<CategoryDto>(client, "api/Category")
                    ?? new List<CategoryDto>();

                model.RecentEnrolments =
                    await GetListAsync<EnrolmentDto>(client, "api/Enrolment")
                    ?? new List<EnrolmentDto>();

                model.RecentResults =
                    await GetListAsync<ResultDto>(client, "api/Result")
                    ?? new List<ResultDto>();

                model.Payments =
                    await GetListAsync<PaymentDto>(client, "api/Payment")
                    ?? new List<PaymentDto>();

                // Calculate revenue from paid payments
                model.TotalRevenue = model.Payments
                    .Where(p => string.Equals(
                        p.PaymentStatus,
                        "Paid",
                        StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
            }

            return View(model);
        }

        private async Task<List<T>?> GetListAsync<T>(
            HttpClient client,
            string path)
        {
            try
            {
                return await client.GetFromJsonAsync<List<T>>(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not retrieve data from {Path}",
                    path);

                return null;
            }
        }

        private async Task<int> CountAsync(
            HttpClient client,
            string path)
        {
            try
            {
                var items =
                    await client.GetFromJsonAsync<List<object>>(path);

                return items?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not retrieve count from {Path}",
                    path);

                return 0;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}