using Microsoft.AspNetCore.Mvc;
using Marathon.MVC.Attributes;
using Marathon.MVC.Models;
using Marathon .MVC.Services;

namespace Marathon.MVC.Controllers;

[SessionAuthorize]
public class EventsController : Controller
{
    private readonly ApiService _api;

    public EventsController(ApiService api) => _api = api;

    
    public async Task<IActionResult> Index(string? eventType)
    {
        var url = "api/events" + (string.IsNullOrEmpty(eventType) ? "" : $"?eventType={eventType}");
        var events = await _api.GetAsync<List<EventViewModel>>(url) ?? new();
        ViewBag.EventType = eventType;
        return View(events);
    }

    // 2. GET: /Events/Details/1
    public async Task<IActionResult> Details(int id)
    {
        
        if (id == 0) return Content("Error: The Event ID was 0. The link on the Index page is broken.");

        var ev = await _api.GetAsync<EventViewModel>($"api/events/{id}");

        if (ev == null) return NotFound();

        return View(ev);
    }

    
    [SessionAuthorize(Role = "Participant")]
    [HttpPost]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (eventId == 0)
        {
            return Content("Error: The form did not send a valid Event ID.");
        }

        var (success, error) = await _api.PostAsync("api/enrolments",
            new { EventId = eventId, CategoryId = categoryId });

        if (success)
        {
            TempData["Success"] = "Enrolment submitted!";
            return RedirectToAction("MyEnrolments");
        }

        TempData["Error"] = $"Enrolment failed: {error}";
        return RedirectToAction("Details", new { id = eventId });
    }

   
    [SessionAuthorize(Role = "Participant")]
    public async Task<IActionResult> MyEnrolments()
    {
        var enrolments = await _api.GetAsync<List<EnrolmentViewModel>>("api/enrolments/my-enrolments") ?? new();
        return View(enrolments);
    }

   
    [SessionAuthorize(Role = "Participant")]
    public async Task<IActionResult> MyResults()
    {
        var results = await _api.GetAsync<List<ResultViewModel>>("api/results/my-results") ?? new();
        return View(results);
    }
}