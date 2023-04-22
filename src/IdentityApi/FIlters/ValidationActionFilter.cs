namespace IdentityApi.Filters;

public class ValidationActionFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (!context.ModelState.IsValid)
		{
			var errors = new Dictionary<string, IEnumerable<string>>();
			
			foreach(var (key, item) in context.ModelState)
			{
				errors[key] = item.Errors.Select(error => error.ErrorMessage).ToList();
			}
			
			context.Result = new BadRequestObjectResult(new FailResponse { Errors = errors });
			return;
		}
		
		await next.Invoke();
	}
}