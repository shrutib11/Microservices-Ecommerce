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

    public static string EncodeFromInt(this int id, IServiceProvider services)
    {
        var hashids = GetHashids(services);
        return hashids.Encode(id);
    }
}
