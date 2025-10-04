using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaVotacao.Models;
using SistemaVotacao.Filters;

namespace SistemaVotacao.Controllers
{
    [SessionAuthorize]
    public class EleitoresController : Controller
    {
        private readonly string _connectionString;

        public EleitoresController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // LISTAGEM - Permitido para todos os usuários logados
        public IActionResult Index()
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

        // DETALHES - Permitido para todos os usuários logados
        public IActionResult Details(int id)
        {
            Eleitores eleitor = null;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarEleitores", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_eleitor", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                eleitor = new Eleitores
                                {
                                    id_eleitor = reader.GetInt32("id_eleitor"),
                                    nome = reader.GetString("nome"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    criado_em = reader.GetDateTime("criado_em")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            if (eleitor == null)
            {
                return NotFound();
            }
            return View(eleitor);
        }

        // CRIAÇÃO - Apenas Admin e Gerente
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Create(Eleitores eleitor)
        {
            if (!ModelState.IsValid)
            {
                return View(eleitor);
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("CriarEleitor", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_nome", eleitor.nome);
                        command.Parameters.AddWithValue("p_titulo_eleitoral", eleitor.titulo_eleitoral);

                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(eleitor);
            }
        }

        // EDIÇÃO - Apenas Admin e Gerente
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Edit(int id)
        {
            Eleitores eleitor = null;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarEleitores", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_eleitor", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                eleitor = new Eleitores
                                {
                                    id_eleitor = reader.GetInt32("id_eleitor"),
                                    nome = reader.GetString("nome"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    criado_em = reader.GetDateTime("criado_em")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            if (eleitor == null)
            {
                return NotFound();
            }
            return View(eleitor);
        }

        [HttpPost]
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Edit(int id, Eleitores eleitor)
        {
            if (id != eleitor.id_eleitor)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(eleitor);
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("EditarEleitor", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_eleitor", eleitor.id_eleitor);
                        command.Parameters.AddWithValue("p_nome", eleitor.nome);
                        command.Parameters.AddWithValue("p_titulo_eleitoral", eleitor.titulo_eleitoral);

                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(eleitor);
            }
        }

        // EXCLUSÃO - Apenas Admin e Gerente
        [HttpPost]
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Delete(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ExcluirEleitor", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_eleitor", id);

                        command.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true, message = "Eleitor excluído com sucesso!" });
            }
            catch (MySqlException ex) when (ex.Number == 1644) // SQLSTATE 45000
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao excluir eleitor: " + ex.Message });
            }
        }
    }
}