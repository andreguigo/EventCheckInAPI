namespace EventCheckInAPI.Models
{
    public class Inscricao
    {
        public Guid Id { get; set; }
        public string? QrCode { get; set; }
        public string? Pin { get; set; }
        public Guid EventoId { get; set; }
        public Guid UsuarioId { get; set; }

        public string CriarPin()
        {
            Random random = new Random();
            return new string(Enumerable.Range(0, 8)
                                .Select(_ => (char)('0' + random.Next(0, 10)))
                                .ToArray());
        }

        public bool RealizarCheckin(string qrCode, string pin)
        {
            return QrCode == qrCode || Pin == pin;
        }
    }
}