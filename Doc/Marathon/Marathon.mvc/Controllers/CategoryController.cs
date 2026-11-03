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

        
        public async Task<IActionResult> Index()
        {
            var model = await GetListAsync<CategoryDto>("api/Category") ?? new List<CategoryDto>();
            return View(model);
        }

     
        [Authorize(Roles = "Admin")] // ?? Locks it down!
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryDto());
        }
        [Authorize(Roles = "Admin,Organiser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] NewCategoryRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid category information.");

            var client = _httpFactory.CreateClient("ApiClient");

            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await client.PostAsJsonAsync(
                    "api/Category",
                    model
                );

                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return Ok(result);

                _logger.LogWarning(
                    "Category creation failed: {StatusCode} {Error}",
                    response.StatusCode,
                    result
                );

                return StatusCode((int)response.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category.");

                return StatusCode(500, "Could not connect to the API.");
            }
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