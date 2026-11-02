using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.mvc.Controllers
{
    [Authorize] // Everyone logged in can see the list
    public class CategoryController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IHttpClientFactory httpFactory, ILogger<CategoryController> logger)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        // 1. GET ALL (Available to all logged-in users)
        public async Task<IActionResult> Index()
        {
            var model = await GetListAsync<CategoryDto>("api/Category") ?? new List<CategoryDto>();
            return View(model);
        }

        // 2. CREATE (Admin Only)
        [Authorize(Roles = "Admin")] // ?? Locks it down!
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryDto());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpFactory.CreateClient("ApiClient");
            var request = new HttpRequestMessage(HttpMethod.Post, "api/Category");
            request.Content = JsonContent.Create(model);

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Failed to create category: {error}");
            return View(model);
        }

        // 3. DELETE (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = _httpFactory.CreateClient("ApiClient");
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Category/{id}");

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Category deleted.";
            }
            else
            {
                TempData["Error"] = "Failed to delete category.";
            }

            return RedirectToAction(nameof(Index));
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
                    _logger.LogWarning("API call failed: {StatusCode} {Path}", response.StatusCode, path);
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