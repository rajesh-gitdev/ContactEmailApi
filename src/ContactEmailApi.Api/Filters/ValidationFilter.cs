using ContactEmailApi.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ContactEmailApi.Api.Filters;

/// <summary>
/// Runs any registered FluentValidation validator against each action argument before the
/// action executes. On failure it throws <see cref="ValidationAppException"/>, which the
/// global exception middleware maps to a 422 response using the standard envelope.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(e => e.ErrorMessage).ToArray();
                    throw new ValidationAppException(errors);
                }
            }
        }

        await next();
    }
}
