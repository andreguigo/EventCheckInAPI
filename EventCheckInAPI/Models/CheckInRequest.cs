namespace EventCheckInAPI.Models
{
    public class CheckInRequest
    {
        public string? QrCode { get; set; }
        public string? Pin { get; set; }
        public Guid EventoId { get; set; }
    }
}