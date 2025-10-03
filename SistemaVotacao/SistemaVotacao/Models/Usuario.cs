using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace SistemaVotacao.Models
{
    public class Usuario
    {
        public int id { get; set; }
        [Required]
        [StringLength(100)]
        public string nome { get; set; }
        [Required]
        [StringLength(20)]
        public string titulo_eleitoral { get; set; }
        [Required]
        [StringLength(255)]
        public string senha_hash { get; set; }
        [Required]
        public string role { get; set; }
        public bool ativo { get; set; }
        public DateTime criado_em { get; set; }     
      
    }
}
