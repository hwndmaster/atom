global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Reporting.UI.RichDocuments;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Reporting.UI;

[ExcludeFromCodeCoverage]
public static class Module
{
    private static IServiceProvider? _serviceProvider;
    internal static IServiceProvider ServiceProvider
        => _serviceProvider ?? throw new ArgumentNullException("Call Genius.Atom.Reporting.UI.Module.Initialize(serviceProvider) in your application initialization.");

    public static void Configure(IServiceCollection services)
    {
        services.AddTransient<IFlowDocumentConverter, FlowDocumentConverter>();
        services.AddTransient<IRichBlockConverter, ParagraphRichBlockConverter>();
        services.AddTransient<IRichBlockConverter, ListRichBlockConverter>();
        services.AddTransient<IInlineRichBlockConverter, HyperlinkTextRichBlockConverter>();
        services.AddTransient<IInlineRichBlockConverter, LineBreakInlineRichBlockConverter>();
        services.AddTransient<IInlineRichBlockConverter, TextInlineRichBlockConverter>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull();
    }
}
