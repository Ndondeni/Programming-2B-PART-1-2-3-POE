namespace Marathon.mvc.Models.dto
{
    public class NewCategoryRequest
    {
        public Guid EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public string GenderRestrictions { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public string CreatedBy { get; set; } = "system";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}