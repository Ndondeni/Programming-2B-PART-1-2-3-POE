using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbcontext dbcontext;
        private readonly BlobServiceClient _blobService;

        public ResultsController(ApplicationDbcontext dbcontext, BlobServiceClient blobService)
        {
            this.dbcontext = dbcontext;
            _blobService = blobService;
        }

        // ─────────────────────────────────────────────
        // GET: api/results
        // Admin / Organiser only
        // ─────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> GetAllResults()
        {
            var results = await dbcontext.results
                .Select(r => new
                {
                    r.ResultId,
                    r.Winner,
                    r.ParticipantEventId,
                    r.finish_position,
                    r.finish_time_seconds,
                    r.pace_pace_km,
                    r.overall_rank,
                    r.prize_money,
                    r.category_rank
                })
                .ToListAsync();

            return Ok(results);
        }

        // ─────────────────────────────────────────────
        // GET: api/results/{id}
        // ─────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResultById(Guid id)
        {
            var result = await dbcontext.results.FindAsync(id);

            if (result == null)
                return NotFound($"Result with id {id} not found");

            return Ok(result);
        }

        // ─────────────────────────────────────────────
        // GET: api/results/enrolment/{participantEventId}
        // ─────────────────────────────────────────────
        [HttpGet("enrolment/{participantEventId}")]
        public async Task<IActionResult> GetResultsForEnrolment(Guid participantEventId)
        {
            var enrolment = await dbcontext.participant_Events
                .FindAsync(participantEventId);

            if (enrolment == null)
                return NotFound($"Enrolment with id {participantEventId} not found");

            var results = await dbcontext.results
                .Where(r => r.ParticipantEventId == participantEventId)
                .ToListAsync();

            return Ok(new
            {
                participantEventId,
                count = results.Count,
                results
            });
        }

        [HttpPost("{raceId:int}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadEventImage(int raceId, IFormFile file)
        {
            // 1. Find the event (using events, not courses)
            var raceEvent = await dbcontext.results.FindAsync(raceId);
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
                RaceId = raceEvent.ResultId,
                BlobName = blobName
            });
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var enrolment = await dbcontext.participant_Events
                .FindAsync(dto.ParticipantEventId);

            if (enrolment == null)
                return NotFound($"Enrolment with id {dto.ParticipantEventId} not found");

            var exists = await dbcontext.results
                .AnyAsync(r => r.ParticipantEventId == dto.ParticipantEventId);

            if (exists)
                return BadRequest("A result for this enrolment already exists");

            var result = new Result
            {
                ParticipantEventId  = dto.ParticipantEventId,
                Winner = dto.Winner,
                finish_position     = dto.finish_position,
                finish_time_seconds = dto.finish_time_seconds,
                pace_pace_km        = dto.pace_pace_km,
                overall_rank        = dto.overall_rank,
                prize_money         = dto.prize_money,
                category_rank       = dto.category_rank
            };

            dbcontext.results.Add(result);
            await dbcontext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResultById),
                new { id = result.ResultId },
                result);
        }

        // ─────────────────────────────────────────────
        // PUT: api/results/{id}
        // Admin / Organiser only
        // ─────────────────────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> UpdateResult(Guid id, [FromBody] UpdateResultDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await dbcontext.results.FindAsync(id);
            if (result == null)
                return NotFound($"Result with id {id} not found");

            result.finish_position     = dto.finish_position;
            result.finish_time_seconds = dto.finish_time_seconds;
            result.pace_pace_km        = dto.pace_pace_km;
            result.overall_rank        = dto.overall_rank;
            result.prize_money         = dto.prize_money;
            result.category_rank       = dto.category_rank;

            await dbcontext.SaveChangesAsync();

            return Ok(result);
        }

        
        // Admin only
       
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            var result = await dbcontext.results.FindAsync(id);
            if (result == null)
                return NotFound($"Result with id {id} not found");

            dbcontext.results.Remove(result);
            await dbcontext.SaveChangesAsync();

            return NoContent();
        }
    }
}