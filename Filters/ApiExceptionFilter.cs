using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Cloud.Filters
{
	public class ApiExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<ApiExceptionFilter> _logger;

		public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
		{
			_logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			_logger.LogError(context.Exception, "An unhandled exception occurred.");

			context.Result = new ObjectResult(new { error = "An unexpected error occurred." })
			{
				StatusCode = StatusCodes.Status500InternalServerError
			};
			context.ExceptionHandled = true;
		}
	}
}