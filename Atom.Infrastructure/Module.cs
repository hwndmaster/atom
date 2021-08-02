using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class Module
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IEventBus, EventBus>();
        }
    }
}
