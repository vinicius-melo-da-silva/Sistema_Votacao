using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaVotacao.Models;
using SistemaVotacao.Filters;

namespace SistemaVotacao.Controllers
{
    [SessionAuthorize(RoleAnyOf = "Adm")]
    public class UsuariosController : Controller
    {
        private readonly string _connectionString;

        public UsuariosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Todas as ações neste controller são restritas apenas ao Admin
        public IActionResult Index()
        {
            var usuarios = new List<Usuario>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarUsuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id", null);
                        command.Parameters.AddWithValue("p_ativos_apenas", false);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                usuarios.Add(new Usuario
                                {
                                    id = reader.GetInt32("id"),
                                    nome = reader.GetString("nome"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    role = reader.GetString("role"),
                                    ativo = reader.GetBoolean("ativo"),
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

            return View(usuarios);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("CriarUsuario", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_nome", usuario.nome);
                        command.Parameters.AddWithValue("p_titulo_eleitoral", usuario.titulo_eleitoral);
                        command.Parameters.AddWithValue("p_senha_hash", usuario.senha_hash); // Em produção, hash a senha!
                        command.Parameters.AddWithValue("p_role", usuario.role);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Usuário cadastrado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(usuario);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarUsuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id", id);
                        command.Parameters.AddWithValue("p_ativos_apenas", false);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var usuario = new Usuario
                                {
                                    id = reader.GetInt32("id"),
                                    nome = reader.GetString("nome"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    role = reader.GetString("role"),
                                    ativo = reader.GetBoolean("ativo"),
                                    criado_em = reader.GetDateTime("criado_em")
                                };
                                return View(usuario);
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
        public IActionResult Edit(int id, Usuario usuario)
        {
            if (id != usuario.id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("EditarUsuario", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id", usuario.id);
                        command.Parameters.AddWithValue("p_nome", usuario.nome);
                        command.Parameters.AddWithValue("p_senha_hash", usuario.senha_hash); // Em produção, verificar se a senha foi alterada e fazer hash!
                        command.Parameters.AddWithValue("p_role", usuario.role);
                        command.Parameters.AddWithValue("p_ativo", usuario.ativo);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Usuário atualizado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(usuario);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ExcluirUsuario", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id", id);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Usuário excluído com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao excluir usuário: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}