namespace Marathon.mvc.Models.dto
{
    public class CreateEventViewModel
    {
        public required string EventName { get; set; }
        public required DateOnly EventDate { get; set; }
        public required string Location { get; set; }
        public required string Venue { get; set; }
        public required float DistanceKm { get; set; }
        public required string EventType { get; set; }
        public required float EventFee { get; set; }
        public required string Status { get; set; }
    }
}