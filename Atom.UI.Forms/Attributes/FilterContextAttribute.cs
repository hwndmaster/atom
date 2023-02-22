using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines the selected property to hold a filter value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FilterContextAttribute : Attribute
{
    /// <summary>
    ///   Gets or sets an optional scope name for the filter context. Used in conjunction
    ///   with the <see cref="IAutoGridContextBuilder{TViewModel}.WithFilterContextScope(string)"/> in the AutoGrid builder.
    /// </summary>
    /// <example>
    ///   In the parent view model:
    ///   <code>
    ///   [FilterContext(Scope = "SpecificScopeName")]
    ///   public string Filter
    ///   {
    ///     get => GetOrDefault<string>();
    ///     set => RaiseAndSetIfChanged(value);
    ///   }
    ///   </code>
    ///
    ///   In the AutoGrid builder:
    ///   <code>
    ///   _contextBuilderFactory.Create()
    ///      .WithColumns(...)
    ///      .WithFilterContextScope("SpecificScopeName")
    ///      .Build()
    ///   </code>
    /// </example>
    public string? Scope { get; set; }
}
