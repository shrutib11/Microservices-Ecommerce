using HashidsNet;
using Microsoft.Extensions.DependencyInjection;

namespace Microservices.Shared.Helpers;

public static class HashidsHelper
{
    private static IHashids GetHashids(IServiceProvider services)
    {
        return services.GetRequiredService<IHashids>();
    }

    public static int? DecodeToInt(this string hashedId, IServiceProvider services)
    {
        var hashids = GetHashids(services);
        var decoded = hashids.Decode(hashedId);

        return decoded.Length > 0 ? decoded[0] : null;
    }

    public static string EncodeEmail(this string email)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(email);
        return Convert.ToBase64String(bytes);
    }

    public static string DecodeEmail(this string encodedEmail)
    {
        var bytes = Convert.FromBase64String(encodedEmail);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static string EncodeFromInt(this int id, IServiceProvider services)
    {
        var hashids = GetHashids(services);
        return hashids.Encode(id);
    }
}
