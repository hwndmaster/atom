﻿using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Commands;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Io;
using Genius.Atom.Infrastructure.Net;
using Genius.Atom.Infrastructure.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Module
{
    private static IServiceProvider? _serviceProvider;
    internal static IServiceProvider ServiceProvider
        => _serviceProvider ?? throw new ArgumentNullException("Call Genius.Atom.Infrastructure.Module.Initialize(serviceProvider) in your application initialization.");

    public static void Configure(IServiceCollection services)
    {
        services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
        services.AddSingleton(typeof(IFactory<>), typeof(ServiceFactory<>));
        services.AddSingleton<ICommandBus, CommandBus>();
        services.AddSingleton<IDateTime, SystemDateTime>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<IFileService, FileService>();
        services.AddTransient<IFileSystemWatcher, FileSystemWatcherWrapper>();
        services.AddSingleton<ITrickyHttpClient, TrickyHttpClient>();
        services.AddTransient<ISynchronousScheduler, SynchronousScheduler>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull();
    }
}
