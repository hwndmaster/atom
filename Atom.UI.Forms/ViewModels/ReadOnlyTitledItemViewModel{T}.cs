namespace Genius.Atom.UI.Forms;

public sealed class ReadOnlyTitledItemViewModel<TModel> : ReadOnlyTitledItemViewModel
{
    public ReadOnlyTitledItemViewModel(Guid id, string name, TModel model)
        : base(id, name)
    {
        Model = model;
    }

    public TModel Model { get; }
}
