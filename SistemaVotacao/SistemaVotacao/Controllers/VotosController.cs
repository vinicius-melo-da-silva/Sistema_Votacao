using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaVotacao.Models;
using SistemaVotacao.Filters;

namespace SistemaVotacao.Controllers
{
    [SessionAuthorize]
    public class VotosController : Controller
    {
        private readonly string _connectionString;

        public VotosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // LISTAGEM - Acesso diferenciado por role
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");
            var tituloEleitoral = HttpContext.Session.GetString("TituloEleitoral");

            // Para usuários comuns, mostrar apenas seu voto
            if (userRole == "Comum")
            {
                return RedirectToAction("MeuVoto");
            }

            // Para Admin e Gerente, mostrar todos os votos
            var votos = new List<dynamic>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarVotos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_voto", null);
                        command.Parameters.AddWithValue("p_id_eleitor", null);
                        command.Parameters.AddWithValue("p_id_candidato", null);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                votos.Add(new
                                {
                                    id_voto = reader.GetInt32("id_voto"),
                                    id_eleitor = reader.GetInt32("id_eleitor"),
                                    nome_eleitor = reader.GetString("nome_eleitor"),
                                    titulo_eleitor = reader.GetString("titulo_eleitor"),
                                    id_candidato = reader.GetInt32("id_candidato"),
                                    nome_candidato = reader.GetString("nome_candidato"),
                                    data_voto = reader.GetDateTime("data_voto"),
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

            ViewBag.UserRole = userRole;
            return View(votos);
        }

        // MEU VOTO - Para usuários comuns visualizarem seu voto
        public IActionResult MeuVoto()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tituloEleitoral = HttpContext.Session.GetString("TituloEleitoral");

            try
            {
                // Primeiro, obter o id_eleitor baseado no título eleitoral
                int? idEleitor = null;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id_eleitor FROM Eleitores WHERE titulo_eleitoral = @tituloEleitoral";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tituloEleitoral", tituloEleitoral);
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            idEleitor = Convert.ToInt32(result);
                        }
                    }
                }

                if (idEleitor.HasValue)
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new MySqlCommand("ListarVotos", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("p_id_voto", null);
                            command.Parameters.AddWithValue("p_id_eleitor", idEleitor);
                            command.Parameters.AddWithValue("p_id_candidato", null);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var voto = new
                                    {
                                        id_voto = reader.GetInt32("id_voto"),
                                        id_eleitor = reader.GetInt32("id_eleitor"),
                                        nome_eleitor = reader.GetString("nome_eleitor"),
                                        titulo_eleitor = reader.GetString("titulo_eleitor"),
                                        id_candidato = reader.GetInt32("id_candidato"),
                                        nome_candidato = reader.GetString("nome_candidato"),
                                        data_voto = reader.GetDateTime("data_voto"),
                                        criado_em = reader.GetDateTime("criado_em")
                                    };
                                    return View(voto);
                                }
                            }
                        }
                    }
                }

                // Se não encontrou voto, redireciona para votar
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // VOTAR - Acesso para todos os usuários logados
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tituloEleitoral = HttpContext.Session.GetString("TituloEleitoral");

            // Verificar se o usuário já votou
            try
            {
                int? idEleitor = null;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id_eleitor FROM Eleitores WHERE titulo_eleitoral = @tituloEleitoral";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tituloEleitoral", tituloEleitoral);
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            idEleitor = Convert.ToInt32(result);
                        }
                    }

                    if (idEleitor.HasValue)
                    {
                        // Verificar se já existe voto
                        var verificaVotoQuery = "SELECT COUNT(*) FROM Votos WHERE id_eleitor = @idEleitor";
                        using (var command = new MySqlCommand(verificaVotoQuery, connection))
                        {
                            command.Parameters.AddWithValue("@idEleitor", idEleitor);
                            var votoCount = Convert.ToInt32(command.ExecuteScalar());
                            if (votoCount > 0)
                            {
                                TempData["Warning"] = "Você já realizou seu voto!";
                                return RedirectToAction("MeuVoto");
                            }
                        }
                    }
                }

                // Carregar lista de candidatos para votação
                var candidatos = new List<Candidatos>();
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
                                    foto = reader.IsDBNull("foto") ? null : reader.GetString("foto")
                                });
                            }
                        }
                    }
                }

                ViewBag.Candidatos = candidatos;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Create(int idCandidato)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tituloEleitoral = HttpContext.Session.GetString("TituloEleitoral");
            var userName = HttpContext.Session.GetString("UserName");

            try
            {
                // Obter id_eleitor baseado no título eleitoral
                int? idEleitor = null;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id_eleitor FROM Eleitores WHERE titulo_eleitoral = @tituloEleitoral";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tituloEleitoral", tituloEleitoral);
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            idEleitor = Convert.ToInt32(result);
                        }
                    }

                    if (!idEleitor.HasValue)
                    {
                        ViewBag.Error = "Eleitor não encontrado!";
                        return View();
                    }

                    // Registrar o voto
                    using (var command = new MySqlCommand("RegistrarVoto", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_eleitor", idEleitor);
                        command.Parameters.AddWithValue("p_id_candidato", idCandidato);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Voto registrado com sucesso! Obrigado por participar da eleição.";
                return RedirectToAction("MeuVoto");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                // Recarregar candidatos em caso de erro
                var candidatos = new List<Candidatos>();
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
                                    foto = reader.IsDBNull("foto") ? null : reader.GetString("foto")
                                });
                            }
                        }
                    }
                }

                ViewBag.Candidatos = candidatos;
                return View();
            }
        }

        // EDIÇÃO DE VOTO - Apenas Admin e Gerente
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Edit(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarVotos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_voto", id);
                        command.Parameters.AddWithValue("p_id_eleitor", null);
                        command.Parameters.AddWithValue("p_id_candidato", null);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var voto = new
                                {
                                    id_voto = reader.GetInt32("id_voto"),
                                    id_eleitor = reader.GetInt32("id_eleitor"),
                                    nome_eleitor = reader.GetString("nome_eleitor"),
                                    titulo_eleitor = reader.GetString("titulo_eleitor"),
                                    id_candidato = reader.GetInt32("id_candidato"),
                                    nome_candidato = reader.GetString("nome_candidato"),
                                    data_voto = reader.GetDateTime("data_voto")
                                };

                                // Carregar lista de candidatos
                                var candidatos = new List<Candidatos>();
                                using (var connection2 = new MySqlConnection(_connectionString))
                                {
                                    connection2.Open();
                                    using (var command2 = new MySqlCommand("ListarCandidatos", connection2))
                                    {
                                        command2.CommandType = CommandType.StoredProcedure;
                                        command2.Parameters.AddWithValue("p_id_candidato", null);

                                        using (var reader2 = command2.ExecuteReader())
                                        {
                                            while (reader2.Read())
                                            {
                                                candidatos.Add(new Candidatos
                                                {
                                                    id_candidato = reader2.GetInt32("id_candidato"),
                                                    nome = reader2.GetString("nome")
                                                });
                                            }
                                        }
                                    }
                                }

                                ViewBag.Candidatos = candidatos;
                                ViewBag.Voto = voto;
                                return View();
                            }
                        }
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Edit(int id, int idCandidatoNovo)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("EditarVoto", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_voto", id);
                        command.Parameters.AddWithValue("p_id_candidato_novo", idCandidatoNovo);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Voto atualizado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao atualizar voto: " + ex.Message;
                return RedirectToAction("Edit", new { id });
            }
        }

        // EXCLUSÃO DE VOTO - Apenas Admin e Gerente
        [HttpPost]
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Delete(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ExcluirVoto", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_voto", id);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Voto excluído com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao excluir voto: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}