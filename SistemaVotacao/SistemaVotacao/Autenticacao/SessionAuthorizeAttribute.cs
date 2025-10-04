using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SistemaVotacao.Autenticacao;

namespace SistemaVotacao.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public string? RoleAnyOf { get; set; }
        public bool AllowAnonymous { get; set; } = false;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Se permitir anônimo, não faz verificação
            if (AllowAnonymous)
            {
                base.OnActionExecuting(context);
                return;
            }

            var http = context.HttpContext;
            var role = http.Session.GetString(SessionKeys.UserRole);
            var userId = http.Session.GetInt32(SessionKeys.UserId);

            // Verifica se o usuário está logado
            if (userId == null)
            {
                // Verificar se já não está na página de login para evitar loop
                var actionName = context.RouteData.Values["action"]?.ToString();
                var controllerName = context.RouteData.Values["controller"]?.ToString();

                if (controllerName != "Auth" || actionName != "Login")
                {
                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                    return;
                }
            }

            // Verifica se há restrição de role
            if (!string.IsNullOrWhiteSpace(RoleAnyOf))
            {
                var allowedRoles = RoleAnyOf.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!allowedRoles.Contains(role))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}