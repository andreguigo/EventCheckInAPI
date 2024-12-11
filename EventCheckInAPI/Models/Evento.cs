using Dapper;
using MySqlConnector;

namespace EventCheckInAPI.Models
{
    public class Evento
    {
        public Guid Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }
        public string? PeriodoContinuo { get; set; }
        public string? Local { get; set; }
        public bool Pago { get; set; }
        public int LimiteInscricoes { get; set; }

        public bool VerificarDisponibilidade(int inscritos)
        {
            return inscritos < LimiteInscricoes;
        }

        public IEnumerable<Usuario> GerarRelatorioParticipantes(MySqlConnection db)
        {
            const string query = @"SELECT u.NomeCompleto, u.Email, u.Telefone FROM Inscricoes i 
                                INNER JOIN Usuarios u ON i.UsuarioId = u.Id WHERE i.EventoId = @Id";
            return db.Query<Usuario>(query, new { Id = this.Id });
        }
    }
}