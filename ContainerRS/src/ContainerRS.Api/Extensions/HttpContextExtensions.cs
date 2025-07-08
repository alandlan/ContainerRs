namespace ContainerRS.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static Guid? GetClientId(this HttpContext context)
        {
            var clienteId = context.User.Claims
                .Where(c => c.Type.Equals("ClienteId"))
                .Select(c => c.Value)
                .FirstOrDefault();

            return !string.IsNullOrEmpty(clienteId) ? Guid.Parse(clienteId) : null;
        }
    }
}
