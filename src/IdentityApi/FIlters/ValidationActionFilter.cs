namespace IdentityApi.FIlters;

public class ValidationActionFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (!context.ModelState.IsValid)
		{
			var errorsDic = new Dictionary<string, List<string>>();
			
			foreach(var (key, item) in context.ModelState)
				foreach(var error in item.Errors)
					if (!errorsDic.TryAdd(key, new() { error.ErrorMessage }))
						errorsDic[key].Add(error.ErrorMessage);
			
			context.Result = new BadRequestObjectResult(new { Errors = errorsDic });
			return;
		}
		
		await next.Invoke();
	}
}