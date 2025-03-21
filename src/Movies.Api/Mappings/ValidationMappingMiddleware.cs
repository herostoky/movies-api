using FluentValidation;
using Movies.Contracts.Responses;

namespace Movies.Api.Mappings;

public class ValidationMappingMiddleware
{
    private readonly RequestDelegate _nextDelegate;

    public ValidationMappingMiddleware(RequestDelegate nextDelegate)
    {
        _nextDelegate = nextDelegate;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _nextDelegate(context);
        }
        catch (ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var validationFailureResponse = new ValidationFailureResponse
            {
                Errors = validationException.Errors.Select(x => new ValidationResponse
                {
                    PropertyName = x.PropertyName,
                    Message = x.ErrorMessage
                })
            };

            await context.Response.WriteAsJsonAsync(validationFailureResponse);
        }
    }
}