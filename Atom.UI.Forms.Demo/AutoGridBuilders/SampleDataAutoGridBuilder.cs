using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Demo.ViewModels;

namespace Genius.Atom.UI.Forms.Demo.AutoGridBuilders
{
    public class SampleDataAutoGridBuilder : IAutoGridBuilder
    {
        private readonly IFactory<IAutoGridContextBuilder<SampleData, MainViewModel>> _contextBuilderFactory;

        public SampleDataAutoGridBuilder(IFactory<IAutoGridContextBuilder<SampleData, MainViewModel>> contextBuilderFactory)
        {
            _contextBuilderFactory = contextBuilderFactory.NotNull();
        }

        public IAutoGridContextBuilder Build()
        {
            return _contextBuilderFactory.Create()
                .WithColumns(c => c.AddAll())
                .WithRecordFactory<SampleDataFactory>();
        }
    }
}
