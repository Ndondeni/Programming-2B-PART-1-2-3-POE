// Services/Dashboard/AdminDashboardService.cs
using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.Extensions.Logging;
namespace Marathon.mvc.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AdminDashboardService> _logger;
        public AdminDashboardService(HttpClient http, ILogger<AdminDashboardService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
        {
            var dto = new DashboardDto();

            try
            {
                // Fire all API calls in parallel
                var usersTask = GetListAsync<UserDto>("api/Users", ct);
               
                var eventsTask = GetListAsync<EventDto>("api/Events", ct);
                var categoriesTask = GetListAsync<CategoryDto>("api/Categories", ct);
                var enrolmentsTask = GetListAsync<EnrolmentDto>("api/Enrolments", ct);
                var resultsTask = GetListAsync<ResultDto>("api/Results", ct);
                var paymentsTask = GetListAsync<PaymentDto>("api/Payments", ct);

                await Task.WhenAll(usersTask, eventsTask, categoriesTask,
                                   enrolmentsTask, resultsTask, paymentsTask);

                var users = usersTask.Result;
                var events = eventsTask.Result;
                var categories = categoriesTask.Result;
                var enrolments = enrolmentsTask.Result;
                var results = resultsTask.Result;
                var payments = paymentsTask.Result;
              //  var signUps = signUpsTask.Result;

                // ---- Counts ----
                dto.UserCount = users.Count;
                dto.EventCount = events.Count;
                dto.CategoryCount = categories.Count;
                dto.EnrolmentCount = enrolments.Count;
                dto.ResultCount = results.Count;

                // ---- Financials (only confirmed/paid) ----
                var paid = payments
                    .Where(p => string.Equals(p.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(p.PaymentStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                dto.TotalRevenue = paid.Sum(p => p.Amount);

                // ---- Lists (trim for the dashboard) ----
                var today = DateTime.UtcNow.Date;

                dto.UpcomingEvents = events
                    .Where(e => e.EventDate.ToDateTime(TimeOnly.MinValue) >= today)
                    .OrderBy(e => e.EventDate)
                    .Take(10)
                    .ToList();

                dto.Categories = categories
                    .Take(10)
                    .ToList();


                dto.RecentEnrolments = enrolments
                    .OrderByDescending(e => e.enrolment_date)
                    .Take(10)
                    .ToList();

                dto.RecentResults = results
                    .Take(10)
                    .ToList();

                dto.Payments = payments
                    .OrderByDescending(p => p.PaidAt ?? DateTime.MinValue)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load admin dashboard data");
                throw; // let the controller decide
            }

            return dto;
        }

        private async Task<List<T>> GetListAsync<T>(string path, CancellationToken ct)
        {
            var result = await _http.GetFromJsonAsync<List<T>>(path, ct);
            return result ?? new List<T>();
        }
    }
}
