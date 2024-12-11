namespace EventCheckInAPI.Models
{
    public class Inscricao
    {
        public Guid Id { get; set; }
        public string? QrCode { get; set; }
        public string? Pin { get; set; }
        public Guid EventoId { get; set; }
        public Guid UsuarioId { get; set; }

        public bool RealizarCheckin(string qrCode, string pin)
        {
            return QrCode == qrCode || Pin == pin;
        }
    }
}