using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QA.WidgetPlatform.Api.Application.Exceptions;

namespace QA.WidgetPlatform.Api.Application.Middleware
{
    public class StatusCodeExceptionHandlerMiddleware
    {
        private readonly RequestDelegate request;

        public StatusCodeExceptionHandlerMiddleware(RequestDelegate pipeline)
        {
            this.request = pipeline;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.request(context);
            }
            catch (StatusCodeException exception)
            {
                context.Response.StatusCode = (int)exception.StatusCode;
                context.Response.Headers.Clear();
            }
        }
    }
}
