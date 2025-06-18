using System.Net;
using Serilog;

namespace FormBuilder.Web.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        // Handle exception and log
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            LogException(exception, context);
            await WriteErrorResponse(context);
        }

        // Log the exception details
        private static void LogException(Exception exception, HttpContext context)
        {
            Log.Error(exception, 
                "Error occurred: {Method} {Path} {QueryString}", 
                context.Request.Method,
                context.Request.Path, 
                context.Request.QueryString);
        }

        // Write error response
        private static async Task WriteErrorResponse(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            if (IsApiRequest(context))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"An error occurred\"}");
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }

        // Check if request is API call
        private static bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api");
        }
    }
}
