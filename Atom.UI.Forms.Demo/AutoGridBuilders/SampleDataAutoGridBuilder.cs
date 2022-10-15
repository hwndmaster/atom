using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Demo.ViewModels;

namespace Genius.Atom.UI.Forms.Demo.AutoGridBuilders
{
    public class SampleDataAutoGridBuilder : IAutoGridBuilder
    {
        private readonly IAutoGridContextBuilder<SampleData> _contextBuilder;

        public SampleDataAutoGridBuilder(IAutoGridContextBuilder<SampleData> contextBuilder)
        {
            _contextBuilder = contextBuilder.NotNull();
        }

        public AutoGridBuildContext Build()
        {
            return _contextBuilder
                .WithColumns(c => c.AddAll())
                .WithRecordFactory<SampleDataFactory>()
                .Build();
        }
    }
}
