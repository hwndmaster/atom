using AutoFixture.AutoMoq;

namespace Genius.Atom.Infrastructure.TestingUtil;

public static class InfrastructureTestHelper
{
    public static IDateTime CreateDateTime(DateTime dateTime)
    {
        Mock<IDateTime> mock = new();
        mock.Setup(x => x.Now).Returns(dateTime);
        return mock.Object;
    }

    public static Fixture CreateFixture(int recursionDepth = 2, bool useMutableValueTypeGenerator = false)
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth));

        if (useMutableValueTypeGenerator)
            new SupportMutableValueTypesCustomization().Customize(fixture);

        return fixture;
    }
}
