namespace AccessControl_API.Utilities.Enums
{
    public enum ApiStatus
    {
        ok = 200,
        created = 201,
        noContent = 204,

        badRequest = 400,
        unauthorized = 401,
        forbidden = 403,
        notFound = 404,
        conflict = 409,

        internalServerError = 500
    }
}
