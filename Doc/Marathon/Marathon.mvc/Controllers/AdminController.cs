using Marathon.mvc.Models.dto;
using Marathon.mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.mvc.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminDashboardService _dashboard;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IAdminDashboardService dashboard, ILogger<AdminController> logger)
        {
            _dashboard = dashboard;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        { try
            {
                var dto = await _dashboard.GetDashboardAsync(ct);
                return View(dto);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again later.";
                return View(new DashboardDto());
            }

        }
    }
}
        