using System.ComponentModel.DataAnnotations;

namespace SistemaVotacao.Models
{
    public class Eleitores
    {
        public int id_eleitor { get; set; }
        [Required]
        [StringLength(100)]
        public string nome { get; set; }
        [Required]
        [StringLength(20)]
        public string titulo_eleitoral { get; set; }
        public DateTime criado_em { get; set; }
    }
}
