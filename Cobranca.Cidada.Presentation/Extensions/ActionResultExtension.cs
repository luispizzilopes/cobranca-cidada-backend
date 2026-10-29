using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Cobranca.Cidada.Presentation.Extensions;

public static class ActionResultExtension
{
    public static IActionResult ToActionResult<T>(this T result, ControllerBase controller)
    {
        if(result is null) return controller.NoContent();
        
        return controller.Ok(result);
    }    
    
    public static IActionResult ToActionResult<T>(this ErrorOr<T> result, ControllerBase controller)
    {
        if (result.Value is null && result.IsError == false) 
            return controller.NoContent();
        
        if (!result.IsError)
        {
            return !(result.Value is Success) ?
                controller.Ok(result.Value) :
                controller.Ok("Operação realizada com sucesso!");
        }

        var error = result.FirstError;

        if (error.NumericType == 429)
        {
            if (error.Metadata is not null && error.Metadata.TryGetValue("retryAfterSeconds", out var retryAfter))
                controller.Response.Headers.RetryAfter = Convert.ToString(retryAfter, System.Globalization.CultureInfo.InvariantCulture);

            return controller.StatusCode(StatusCodes.Status429TooManyRequests, error);
        }

        if (error.NumericType == 503)
            return controller.StatusCode(StatusCodes.Status503ServiceUnavailable, error);

        return error.Type switch
        {
            ErrorType.Failure => controller.StatusCode(500, error),
            ErrorType.NotFound => controller.NotFound(error),
            ErrorType.Validation => controller.BadRequest(error),
            ErrorType.Unauthorized => controller.Unauthorized(error),
            ErrorType.Conflict => controller.Conflict(error),
            ErrorType.Forbidden => controller.StatusCode(403, error),
            _ => controller.StatusCode(500, error),
        };
    }
}
