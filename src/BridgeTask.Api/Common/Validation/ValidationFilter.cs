using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BridgeTask.Api.Common.Validation;


/// Runs the registered FluentValidation validator for every action argument
/// and short-circuits with a 400 ValidationProblemDetails on failure.

public sealed class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument), context.HttpContext.RequestAborted);

            foreach (var failure in result.Errors)
            {
                if (!errors.TryGetValue(failure.PropertyName, out var messages))
                    errors[failure.PropertyName] = messages = [];
                messages.Add(failure.ErrorMessage);
            }
        }

        if (errors.Count > 0)
        {
            var problem = new ValidationProblemDetails(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };
            context.Result = new BadRequestObjectResult(problem);
            return;
        }

        await next();
    }
}
