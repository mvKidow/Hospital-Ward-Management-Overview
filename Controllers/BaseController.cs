using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Connect.Controllers
{
    public abstract class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                ViewData["Layout"] = GetLayoutByRole(role);
            }
        }

        private string GetLayoutByRole(string role)
        {
            switch (role)
            {
                case "Admin":
                    return "Admin/Index";
                case "Doctor":
                    return "Doctor/Index";
                case "Nurse":
                    return "Nurse/Index";
                case "WardAdmin":
                    return "WardAdmin/Index";
                default:
                    return "Home/Index";
            }
        }
    }
}