using Microsoft.AspNetCore.Mvc;
using SistemaVotacao.Filters;

namespace SistemaVotacao.Controllers
{
    [SessionAuthorize]
    public class HomeController : Controller
    {
        // Dashboard - Acesso para todos os usuários logados
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString(Autenticacao.SessionKeys.UserRole);
            ViewBag.UserRole = userRole;
            ViewBag.UserName = HttpContext.Session.GetString(Autenticacao.SessionKeys.UserName);

            return View();
        }
    }
}