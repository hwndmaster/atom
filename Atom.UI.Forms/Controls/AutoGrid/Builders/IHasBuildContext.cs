namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal interface IHasBuildContext
{
    /// <summary>
    ///   Creates a new DataGrid context.
    /// </summary>
    AutoGridBuildContext Build();
}
