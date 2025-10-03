namespace SistemaVotacao.Models
{
    public class Votos
    {
        public int id_voto { get; set; }
        public int id_eleitor { get; set; }
        public int id_candidato { get; set; }
        public DateTime data_voto { get; set; }
        public DateTime criado_em { get; set; }
    }
}
