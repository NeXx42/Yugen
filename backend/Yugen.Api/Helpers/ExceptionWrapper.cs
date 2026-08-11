namespace Yugen.Api.Helpers;

public static class ExceptionWrapper
{
    public static async Task<IResult> WrapException<T>(Func<Task<T>> demand)
    {
        try
        {
            T? res = await demand();

            if (res == null)
                return Results.Ok();

            return Results.Json(res);
        }
        catch (Exception e)
        {
            return Results.InternalServerError(e.Message);
        }
    }

    public static async Task<IResult> WrapException(Func<Task> demand)
    {
        try
        {
            await demand();
            return Results.Ok();
        }
        catch (Exception e)
        {
            return Results.InternalServerError(e.Message);
        }
    }
}
