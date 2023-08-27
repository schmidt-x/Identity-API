using System;
using System.Net;
using System.Threading.Tasks;
using IdentityApi.Contracts.Responses;
using IdentityApi.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Middleware;

public class ExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger _logger;

	public ExceptionHandlerMiddleware(RequestDelegate next, ILogger logger)
	{
		_next = next;
		_logger = logger;
	}
	
	public async Task InvokeAsync(HttpContext ctx)
	{
		Task? handlerTask = null;
		
		try
		{
			await _next(ctx);
		}
		catch(Exception ex)
		{
			handlerTask = HandleExceptionAsync(ex, ctx);
		}
		
		if (handlerTask != null)
		{
			await handlerTask;
		}
	}
	
	private async Task HandleExceptionAsync(Exception ex, HttpContext ctx)
	{
		var response = ctx.Response;
		string errorKey;
		string errorMessage;
		
		switch(ex)
		{
			case SecurityTokenException secTokenEx:
				_logger.Warning(secTokenEx, "Security token error: {errorMessage}", secTokenEx.Message);
				
				response.StatusCode = (int)HttpStatusCode.Unauthorized;
				errorKey = ErrorKey.Auth;
				errorMessage = ErrorMessage.Unauthorized;
				
				break;
				
			default:
				_logger.Error(ex, "Unexpected error: {errorMessage}", ex.Message);
				
				response.StatusCode = (int)HttpStatusCode.InternalServerError;
				errorKey = ErrorKey.Error;
				errorMessage = ErrorMessage.UnexpectedError;
				
				break;
		}
		
		await response.WriteAsJsonAsync(new FailResponse { Errors = new()
		{
			{ errorKey, new[] { errorMessage } }
		}});
	}
}
