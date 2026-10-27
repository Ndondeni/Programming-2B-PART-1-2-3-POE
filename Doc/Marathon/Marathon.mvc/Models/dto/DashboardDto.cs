
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marathon.mvc.Models.dto
{
    public class DashboardDto
    {
        // Dashboard counts
        public int UserCount { get; set; }
        public int EventCount { get; set; }
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
        public int ResultCount { get; set; }

        // Financial summary
        public decimal TotalRevenue { get; set; }

        // Dashboard lists
        public List<EventDto> UpcomingEvents { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<EnrolmentDto> RecentEnrolments { get; set; } = new();
        public List<ResultDto> RecentResults { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
    }

    public class EventDto
    {
        public required string EventName { get; set; }
        public required DateOnly EventDate { get; set; }
        public required DateTime Now { get; set; }
        public required string Location { get; set; }
        public required string Venue { get; set; }
        public required float DistanceKm { get; set; }
        public required string EventType { get; set; }
        public required float EventFee { get; set; }
        public required string Status { get; set; }
        public required int CreatedBy { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }

    public class CategoryDto
    {
        [ForeignKey("Event")]
        public Guid EventId { get; set; }

        public required string CategoryName { get; set; }
        public required int MinAge { get; set; }
        public required int MaxAge { get; set; }
        public required string GenderRestrictions { get; set; }
        public required int MaxParticipants { get; set; }
        public required string CreatedBy { get; set; }
        public required DateTime CreatedAt { get; set; }
    }

    public class EnrolmentDto
    {
        public required Guid UserId { get; set; }

        [ForeignKey("Event")]
        public required Guid EventId { get; set; }

        [ForeignKey("Category")]
        public required Guid CategoryId { get; set; }

        public required DateOnly enrolment_date { get; set; }
        public required string enrolment_status { get; set; }
        public required string payment_status { get; set; }
        public required decimal amount_paid { get; set; }
        public required DateOnly paid_date { get; set; }
    }

    public class PaymentDto
    {
        public int EnrolmentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public string? Reference { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class ResultDto
    {
        public Guid ResultId { get; set; } = Guid.NewGuid();

        [ForeignKey("ParticipantEventId")]
        public required Guid ParticipantEventId { get; set; }

        public required string Winner { get; set; }
        public required string finish_position { get; set; }
        public required string finish_time_seconds { get; set; }
        public required string pace_pace_km { get; set; }
        public required string overall_rank { get; set; }
        public required decimal prize_money { get; set; }
        public required string category_rank { get; set; }
    }

    public class Participant_EventDto
    {
        [Key]
        public Guid Participant_EventId { get; set; } = Guid.NewGuid();

        [ForeignKey("User")]
        public required Guid UserId { get; set; }

        [ForeignKey("Event")]
        public required Guid EventId { get; set; }

        [ForeignKey("Category")]
        public required Guid CategoryId { get; set; }

        public required int bib_number { get; set; }
        public required int fining_position { get; set; }
        public required TimeOnly finish_time_seconds { get; set; }
        public required string entry_status { get; set; }
        public required DateOnly entered_at { get; set; }
    }
}
