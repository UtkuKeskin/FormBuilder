using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Web.Filters
{
    public class AllowAnonymousReadAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var user = context.HttpContext.User;
            
            // Allow GET requests for everyone
            if (httpMethod == "GET")
            {
                base.OnActionExecuting(context);
                return;
            }
            
            // Require authentication for other methods
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
            }
        }
    }
}