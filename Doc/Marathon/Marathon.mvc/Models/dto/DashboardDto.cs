using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marathon.mvc.Models.dto
{
    public class DashboardDto
    {
        public int UserCount { get; set; }
        public int EventCount { get; set; }
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
        public int ResultCount { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<UserDto> Users { get; set; } = new();
        public List<EventDto> UpcomingEvents { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<EnrolmentDto> RecentEnrolments { get; set; } = new();
        public List<ResultDto> RecentResults { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateOnly DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
    }

    public class EventDto
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = "";
        public DateOnly EventDate { get; set; }
        public DateTime Now { get; set; }
        public string Location { get; set; } = "";
        public string Venue { get; set; } = "";
        public float DistanceKm { get; set; }
        public string EventType { get; set; } = "";
        public float EventFee { get; set; }
        public string EventStatus { get; set; } = "Draft";
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }

        [ForeignKey("Event")]
        public Guid EventId { get; set; }

        public string CategoryName { get; set; } = "";
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public string GenderRestrictions { get; set; } = "None";
        public int MaxParticipants { get; set; }
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class EnrolmentDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("Event")]
        public Guid EventId { get; set; }

        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }

        public DateOnly enrolment_date { get; set; }
        public string enrolment_status { get; set; } = "Pending";
        public string payment_status { get; set; } = "Unpaid";
        public decimal amount_paid { get; set; }
        public DateOnly paid_date { get; set; }
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
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
        public Guid ParticipantEventId { get; set; }

        public string Winner { get; set; } = "";
        public string finish_position { get; set; } = "";
        public string finish_time_seconds { get; set; } = "";
        public string pace_pace_km { get; set; } = "";
        public string overall_rank { get; set; } = "";
        public decimal prize_money { get; set; }
        public string category_rank { get; set; } = "";
    }

    public class Participant_EventDto
    {
        [Key]
        public Guid Participant_EventId { get; set; } = Guid.NewGuid();

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [ForeignKey("Event")]
        public Guid EventId { get; set; }

        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }

        public int bib_number { get; set; }
        public int fining_position { get; set; }
        public TimeOnly finish_time_seconds { get; set; }
        public string entry_status { get; set; } = "Pending";
        public DateOnly entered_at { get; set; }
    }
}