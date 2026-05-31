using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Genius.Atom.Web;

public static class ReactAppCorsExtensions
{
    private const string ReactAppCorsPolicyName = "AllowReactApp";

    public static WebApplicationBuilder AddReactAppCors(this WebApplicationBuilder builder)
    {
        Guard.NotNull(builder);

        var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        var configuredCorsOrigins = corsOrigins.ToHashSet(StringComparer.OrdinalIgnoreCase);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(ReactAppCorsPolicyName, policy =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    policy.SetIsOriginAllowed(origin => configuredCorsOrigins.Contains(origin)
                        || IsLocalDevelopmentOrigin(origin));
                }
                else
                {
                    policy.WithOrigins(corsOrigins);
                }

                policy.AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return builder;
    }

    public static WebApplication UseReactAppCors(this WebApplication app)
    {
        Guard.NotNull(app);

        app.UseCors(ReactAppCorsPolicyName);
        return app;
    }

    private static bool IsLocalDevelopmentOrigin(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase));
    }
}
