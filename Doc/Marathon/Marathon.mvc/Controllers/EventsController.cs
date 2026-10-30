using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.mvc.Controllers;

public class EventsController : Controller
{
    private readonly IHttpClientFactory _http;
    public EventsController(IHttpClientFactory http) => _http = http;

    private const string Base = "api/Events";
    private HttpClient Client => _http.CreateClient("ApiClient");   // shortcut

    // ---------- READ ----------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await Client.GetFromJsonAsync<List<EventDto>>(Base) ?? new();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var resp = await Client.GetAsync($"{Base}/{id}");
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return NotFound();
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = $"Could not load event ({resp.StatusCode}).";
            return RedirectToAction(nameof(Index));
        }
        var dto = await resp.Content.ReadFromJsonAsync<EventDto>();
        return dto is null ? NotFound() : View(dto);
    }

    // ---------- CREATE ----------
    [HttpGet]
    public IActionResult Create() => View(new EventDto
    {
        EventId = Guid.NewGuid(),
        EventName = "",
        Location = "",
        Venue = "",
        EventType = "",
        EventStatus = "Draft",
        EventDate = DateOnly.FromDateTime(DateTime.Today),
        Now = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        CreatedBy = 0,
        DistanceKm = 0,
        EventFee = 0
    });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var resp = await Client.PostAsJsonAsync(Base, dto);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", $"API error: {resp.StatusCode}");
            return View(dto);
        }
        TempData["Success"] = $"Event '{dto.EventName}' created.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- UPDATE ----------
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var resp = await Client.GetAsync($"{Base}/{id}");
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return NotFound();
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = $"Could not load event ({resp.StatusCode}).";
            return RedirectToAction(nameof(Index));
        }
        var dto = await resp.Content.ReadFromJsonAsync<EventDto>();
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EventDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var resp = await Client.PutAsJsonAsync($"{Base}/{id}", dto);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", $"API error: {resp.StatusCode}");
            return View(dto);
        }
        TempData["Success"] = $"Event '{dto.EventName}' updated.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- DELETE ----------
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await Client.GetFromJsonAsync<EventDto>($"{Base}/{id}");
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var resp = await Client.DeleteAsync($"{Base}/{id}");
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = $"Delete failed ({resp.StatusCode}).";
        }
        else
        {
            TempData["Success"] = "Event deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}