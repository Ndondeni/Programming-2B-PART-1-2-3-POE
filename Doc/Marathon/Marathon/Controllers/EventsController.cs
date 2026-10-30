using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        public readonly ApplicationDbcontext dbcontext;
        private readonly BlobServiceClient _blobService;
        

        public EventsController(ApplicationDbcontext dbcontext, BlobServiceClient blobService)
        {
            this.dbcontext = dbcontext;
            _blobService = blobService;
        }
        [HttpGet]
        public IActionResult GetEvents()
        {
            var events = dbcontext.events.ToList();
            return Ok(events);
        }
        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEventById(Guid id)
        {
            var eventItem = dbcontext.events.Find(id);
            if (eventItem == null) return NotFound($"Event with id {id} was not found.");
            return Ok(eventItem);
        }


        [HttpPost]
        public IActionResult CreateEvent(NewEventDTO newEventDTO)
        {

            var newEvent = new Event
            {
                EventName = newEventDTO.EventName,
                EventDate = newEventDTO.EventDate,
                Location = newEventDTO.Location,
                Venue = newEventDTO.Venue,
                DistanceKm = newEventDTO.DistanceKm,
                EventFee = newEventDTO.EventFee,
                EventType = newEventDTO.EventType,
                EventStatus = "Scheduled",



                // server-controlled
                CreatedBy = 1235, // This should be replaced with the actual Employee ID from the authenticated user "aAdmin"
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Now = DateTime.UtcNow
            };

            dbcontext.events.Add(newEvent);
            dbcontext.SaveChanges();

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = newEvent.EventId },
                newEvent
            );
        }
        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateEvent(Guid id, UpdateEventDTO updatedEventDTO)
        {
            var existingEvent = dbcontext.events.Find(id);
            if (existingEvent == null) return NotFound($"Event with id {id} was not found.");
            existingEvent.EventName = updatedEventDTO.EventName;
            existingEvent.EventDate = updatedEventDTO.EventDate;
            existingEvent.Location = updatedEventDTO.Location;
            existingEvent.Venue = updatedEventDTO.Venue;
            existingEvent.DistanceKm = updatedEventDTO.DistanceKm;
            existingEvent.EventFee = updatedEventDTO.EventFee;
            existingEvent.EventType = updatedEventDTO.EventType;
            existingEvent.UpdatedAt = DateTime.UtcNow;
            existingEvent.EventStatus = updatedEventDTO.EventStatus;
            dbcontext.SaveChanges();
            return Ok("Event was updated successfully.");
        }
        [HttpPost("{eventId:int}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadEventImage(int eventId, IFormFile file)
        {
            // 1. Find the event (using events, not courses)
            var raceEvent = await dbcontext.events.FindAsync(eventId);
            if (raceEvent == null)
                return NotFound("Event not found");

            // 2. Validate the file
            if (file == null || file.Length == 0)
                return BadRequest("Select an image");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Maximum size is 5 MB");

            var allowedContentTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedContentTypes.Contains(file.ContentType))
                return BadRequest("JPEG or PNG only");

            // 3. Upload to Azure Blob Storage
            var container = _blobService.GetBlobContainerClient("event-images");
            await container.CreateIfNotExistsAsync();

            var extension = Path.GetExtension(file.FileName);
            var blobName = $"{Guid.NewGuid():N}{extension}";
            var blob = container.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            });

            // 4. Link back to SQL Server and save
            raceEvent.ImageBlobName = blobName;
            await dbcontext.SaveChangesAsync();

            // 5. Return success (Fixed: this was outside the method in your snippet)
            return Ok(new
            {
                EventId = raceEvent.EventId,
                BlobName = blobName
            });
        }
        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult RemoveEvent(Guid id)
        {
            var existingEvent = dbcontext.events.Find(id);
            if (existingEvent == null) return NotFound($"Event with id {id} was not found.");
            dbcontext.events.Remove(existingEvent);
            dbcontext.SaveChanges();
            return Ok("Event was removed successfully.");
        }
    }
}
