namespace EventCheckInAPI.Models
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string? NomeCompleto { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }

        public bool ValidarDadosInscricao()
        {
            return !string.IsNullOrWhiteSpace(NomeCompleto) &&
                !string.IsNullOrWhiteSpace(Email) &&
                Email.Contains("@");
        }
    }
}