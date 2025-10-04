using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaVotacao.Models;
using SistemaVotacao.Filters;

namespace SistemaVotacao.Controllers
{
    [SessionAuthorize]
    public class CandidatosController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _environment;

        public CandidatosController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _environment = environment;
        }

        // LISTAGEM - Permitido para todos os usuários logados
        public IActionResult Index()
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

        // DETALHES - Permitido para todos os usuários logados
        public IActionResult Details(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarCandidatos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_candidato", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var candidato = new Candidatos
                                {
                                    id_candidato = reader.GetInt32("id_candidato"),
                                    nome = reader.GetString("nome"),
                                    cpf = reader.GetString("cpf"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    foto = reader.IsDBNull("foto") ? null : reader.GetString("foto"),
                                    criado_em = reader.GetDateTime("criado_em")
                                };
                                return View(candidato);
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

        // CRIAÇÃO - Apenas Admin e Gerente
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public async Task<IActionResult> Create(Candidatos candidato, IFormFile foto)
        {
            if (!ModelState.IsValid)
            {
                return View(candidato);
            }

            try
            {
                // Upload da foto se for fornecida
                if (foto != null && foto.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "candidatos");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }

                    candidato.foto = $"/uploads/candidatos/{fileName}";
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("CriarCandidato", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_nome", candidato.nome);
                        command.Parameters.AddWithValue("p_cpf", candidato.cpf);
                        command.Parameters.AddWithValue("p_titulo_eleitoral", candidato.titulo_eleitoral);
                        command.Parameters.AddWithValue("p_foto", candidato.foto);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Candidato cadastrado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(candidato);
            }
        }

        // EDIÇÃO - Apenas Admin e Gerente
        [SessionAuthorize(RoleAnyOf = "Adm,Gerente")]
        public IActionResult Edit(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("ListarCandidatos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_candidato", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var candidato = new Candidatos
                                {
                                    id_candidato = reader.GetInt32("id_candidato"),
                                    nome = reader.GetString("nome"),
                                    cpf = reader.GetString("cpf"),
                                    titulo_eleitoral = reader.GetString("titulo_eleitoral"),
                                    foto = reader.IsDBNull("foto") ? null : reader.GetString("foto"),
                                    criado_em = reader.GetDateTime("criado_em")
                                };
                                return View(candidato);
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
        public async Task<IActionResult> Edit(int id, Candidatos candidato, IFormFile foto)
        {
            if (id != candidato.id_candidato)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(candidato);
            }

            try
            {
                // Upload da nova foto se for fornecida
                if (foto != null && foto.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "candidatos");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }

                    candidato.foto = $"/uploads/candidatos/{fileName}";
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("EditarCandidato", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_candidato", candidato.id_candidato);
                        command.Parameters.AddWithValue("p_nome", candidato.nome);
                        command.Parameters.AddWithValue("p_cpf", candidato.cpf);
                        command.Parameters.AddWithValue("p_titulo_eleitoral", candidato.titulo_eleitoral);
                        command.Parameters.AddWithValue("p_foto", candidato.foto);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Candidato atualizado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(candidato);
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
                    using (var command = new MySqlCommand("ExcluirCandidato", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_id_candidato", id);

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Candidato excluído com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao excluir candidato: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}