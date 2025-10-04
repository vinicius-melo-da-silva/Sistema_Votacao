using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaVotacao.Models;
using SistemaVotacao.Filters;

namespace SistemaVotacao.Controllers
{
    [SessionAuthorize(AllowAnonymous = true)]
    public class PublicController : Controller
    {
        private readonly string _connectionString;

        public PublicController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Resultado da eleição - acesso público
        public IActionResult Resultado()
        {
            var resultados = new List<dynamic>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ObterResultadoEleicao", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultados.Add(new
                                {
                                    id_candidato = reader.GetInt32("id_candidato"),
                                    nome_candidato = reader.GetString("nome_candidato"),
                                    foto = reader.IsDBNull("foto") ? null : reader.GetString("foto"),
                                    total_votos = reader.GetInt32("total_votos"),
                                    percentual = reader.GetDecimal("percentual")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(resultados);
        }

        // Listagem de candidatos - acesso público
        public IActionResult Candidatos()
        {
            var candidatos = new List<Candidatos>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarCandidatos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_candidato", null);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                candidatos.Add(new Candidatos
                                {
                                    id_candidato = reader.GetInt32("id_candidato"),
                                    nome = reader.GetString("nome"),
                                    cpf = reader.GetString("cpf"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    foto = reader.IsDBNull("foto") ? null : reader.GetString("foto"),
                                    criado_em = reader.GetDateTime("criado_em")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(candidatos);
        }

        // Listagem de eleitores - acesso público  
        public IActionResult Eleitores()
        {
            var eleitores = new List<Eleitores>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarEleitores", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_eleitor", null);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                eleitores.Add(new Eleitores
                                {
                                    id_eleitor = reader.GetInt32("id_eleitor"),
                                    nome = reader.GetString("nome"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    criado_em = reader.GetDateTime("criado_em")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(eleitores);
        }
    }
}