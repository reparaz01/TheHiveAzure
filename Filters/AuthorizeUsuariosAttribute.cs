using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TheHiveAzure.Filters
{
    public class AuthorizeUsuariosAttribute : AuthorizeAttribute,
        IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity.IsAuthenticated == false)
            {
                RouteValueDictionary routeLogin =
                    new RouteValueDictionary(new {
                        controller = "Inicio", action = "Login"
                    });
                context.Result =
                    new RedirectToRouteResult(routeLogin);
            }
        }
    }
}
