using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaVotacao.Autenticacao;
using SistemaVotacao.Filters;

namespace SistemaVotacao.Controllers
{
    [SessionAuthorize(AllowAnonymous = true)]
    public class AuthController : Controller
    {
        private readonly string _connectionString;

        public AuthController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Login - Acesso público
        [SessionAuthorize(AllowAnonymous = true)]
        public IActionResult Login()
        {
            // Se já estiver logado, redireciona para Home
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (userId != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [SessionAuthorize(AllowAnonymous = true)]
        public IActionResult Login(string tituloEleitoral, string senha)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // CORREÇÃO: Incluir senha_hash na consulta
                    var query = @"
                        SELECT u.id, u.nome, u.titulo_eleitoral, u.senha_hash, u.role, u.ativo, e.id_eleitor
                        FROM Usuarios u
                        INNER JOIN Eleitores e ON u.titulo_eleitoral = e.titulo_eleitoral
                        WHERE u.titulo_eleitoral = @tituloEleitoral AND u.ativo = 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tituloEleitoral", tituloEleitoral);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // CORREÇÃO: Obter a senha_hash do resultado
                                var senhaHash = reader["senha_hash"].ToString();

                                // Verificação simples da senha (em produção, use BCrypt)
                                if (senha == senhaHash) // Isso é apenas para demo!
                                {
                                    // Configura a sessão
                                    HttpContext.Session.SetInt32(SessionKeys.UserId, reader.GetInt32("id"));
                                    HttpContext.Session.SetString(SessionKeys.UserName, reader.GetString("nome"));
                                    HttpContext.Session.SetString(SessionKeys.TituloEleitoral, reader.GetString("titulo_eleitoral"));
                                    HttpContext.Session.SetString(SessionKeys.UserRole, reader.GetString("role"));

                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                    }
                }

                ViewBag.Error = "Título eleitoral ou senha inválidos!";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Erro ao fazer login: " + ex.Message;
                return View();
            }
        }

        // GET: Registrar
        [SessionAuthorize(AllowAnonymous = true)]
        public IActionResult Registrar()
        {
            return View();
        }

        // POST: Registrar
        [HttpPost]
        [SessionAuthorize(AllowAnonymous = true)]
        public IActionResult Registrar(string nome, string tituloEleitoral, string senha, string confirmarSenha)
        {
            try
            {
                if (senha != confirmarSenha)
                {
                    ViewBag.Error = "As senhas não coincidem!";
                    return View();
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Verifica se o título eleitoral existe na tabela de eleitores
                    var verificaEleitorQuery = "SELECT id_eleitor FROM Eleitores WHERE titulo_eleitoral = @tituloEleitoral";
                    using (var command = new MySqlCommand(verificaEleitorQuery, connection))
                    {
                        command.Parameters.AddWithValue("@tituloEleitoral", tituloEleitoral);
                        var eleitorId = command.ExecuteScalar();

                        if (eleitorId == null)
                        {
                            ViewBag.Error = "Título eleitoral não encontrado!";
                            return View();
                        }
                    }

                    // Cria o usuário com role 'Comum'
                    using (var command = new MySqlCommand("CriarUsuario", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_nome", nome);
                        command.Parameters.AddWithValue("p_titulo_eleitoral", tituloEleitoral);
                        command.Parameters.AddWithValue("p_senha_hash", senha); // Em produção, hash a senha!
                        command.Parameters.AddWithValue("p_role", "Comum");

                        command.ExecuteNonQuery();
                    }
                }

                // Usar TempData para a mensagem de sucesso sobreviver ao redirecionamento
                TempData["Success"] = "Usuário criado com sucesso! Faça login para continuar.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Erro ao registrar: " + ex.Message;
                return View();
            }
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Acesso Negado
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}