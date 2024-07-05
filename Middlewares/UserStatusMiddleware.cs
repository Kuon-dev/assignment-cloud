using Microsoft.AspNetCore.Identity;
using Cloud.Models;

namespace Cloud.Middlewares;

public class UserStatusMiddleware {
  private readonly RequestDelegate _next;

  public UserStatusMiddleware(RequestDelegate next) {
	_next = next;
  }

  public async Task InvokeAsync(HttpContext context, UserManager<UserModel> userManager) {
	if (context.User.Identity == null) throw new InvalidOperationException("User identity is not available.");
	if (context.User.Identity.IsAuthenticated) {
	  var user = await userManager.GetUserAsync(context.User);

	  if (user != null) {
		if (user.IsBanned) {
		  context.Response.StatusCode = StatusCodes.Status403Forbidden;
		  await context.Response.WriteAsync("Your account has been banned.");
		  return;
		}

		if (!user.IsVerified) {
		  context.Response.StatusCode = StatusCodes.Status403Forbidden;
		  await context.Response.WriteAsync("Your account is not verified.");
		  return;
		}
	  }
	}

	await _next(context);
  }
}


public static class UserStatusMiddlewareExtensions {
  public static IApplicationBuilder UseUserStatusMiddleware(this IApplicationBuilder builder) {
	return builder.UseMiddleware<UserStatusMiddleware>();
  }
}