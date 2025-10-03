using System.ComponentModel.DataAnnotations;

namespace SistemaVotacao.Models
{
    public class Candidatos
    {
        public int id_candidato { get; set; }
        [Required]
        [StringLength(100)]
        public string nome { get; set; }
        [Required]
        [StringLength(14)]
        public string cpf { get; set; }
        [Required]
        [StringLength(20)]
        public string titulo_eleitoral { get; set; }
        [StringLength(255)]
        public string foto { get; set; }
        public DateTime criado_em { get; set; }
    }
}
