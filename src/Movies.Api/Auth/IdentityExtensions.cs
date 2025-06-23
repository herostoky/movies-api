namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var userId = context.User.Claims.SingleOrDefault(c => c.Type == "userId");

        if (Guid.TryParse(userId?.Value, out var id))
        {
            return id;
        }

        return null;
    }
}