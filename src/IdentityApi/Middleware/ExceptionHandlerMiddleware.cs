﻿using System.Security;
using Microsoft.Extensions.Logging;

namespace IdentityApi.Middleware;

public class ExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlerMiddleware> _logger;

	public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}
	
	public async Task InvokeAsync(HttpContext ctx)
	{
		Task? handler = null;
		
		try
		{
			await _next(ctx);
		}
		catch(Exception ex)
		{
			handler = HandleExceptionAsync(ex, ctx);
		}
		
		if (handler != null)
		{
			await handler;
		}
	}
	
	private async Task HandleExceptionAsync(Exception ex, HttpContext ctx)
	{
		var response = ctx.Response;
		string errorMessage;
		
		switch(ex)
		{
			case SecurityException sEx: 
				_logger.LogWarning(sEx, sEx.Message);
				response.StatusCode = 401;
				errorMessage = "Go fuck yourself"; // TODO
				break;
				
			default:
				_logger.LogError(ex, ex.Message);
				response.StatusCode = 500;
				errorMessage = "Unexpected error has occured";
				break;
		}
		
		await response.WriteAsJsonAsync(new FailResponse { Errors = new()
		{
			{ "error", new[] { errorMessage } }
		}});
	}
}
