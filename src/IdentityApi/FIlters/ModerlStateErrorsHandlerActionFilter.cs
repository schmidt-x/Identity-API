using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityApi.Filters;

public class ModerlStateErrorsHandlerActionFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (!context.ModelState.IsValid)
		{
			var errors = new Dictionary<string, IEnumerable<string>>();
			
			foreach(var (key, item) in context.ModelState)
			{
				errors.Add(key, item.Errors.Select(error => error.ErrorMessage).ToList());
			}
			
			context.Result = new BadRequestObjectResult(new FailResponse { Errors = errors });
			return;
		}
		
		await next.Invoke();
	}
}