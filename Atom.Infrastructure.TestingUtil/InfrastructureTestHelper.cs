using AutoFixture.AutoFakeItEasy;
using Genius.Atom.Infrastructure.TestingUtil.FixtureExtensions;

namespace Genius.Atom.Infrastructure.TestingUtil;

public static class InfrastructureTestHelper
{
    public static IDateTime CreateDateTime(DateTime dateTime)
    {
        var fake = new IDateTimeImposter();
        fake.Now.Getter().Returns(dateTime);
        return fake.Instance();
    }

    public static Fixture CreateFixture(int recursionDepth = 2, bool useMutableValueTypeGenerator = false)
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoFakeItEasyCustomization());
        fixture.Customizations.Add(new ImmutableListSpecimenBuilder());
        fixture.Customizations.Add(new RandomByteArrayBuilder());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth));

        if (useMutableValueTypeGenerator)
            new SupportMutableValueTypesCustomization().Customize(fixture);

        return fixture;
    }
}
