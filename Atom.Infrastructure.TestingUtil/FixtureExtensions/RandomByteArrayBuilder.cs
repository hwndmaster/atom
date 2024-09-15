using System.Collections.Immutable;
using AutoFixture.Kernel;

namespace Genius.Atom.Infrastructure.TestingUtil.FixtureExtensions;

public class RandomByteArrayBuilder : ISpecimenBuilder
{
    private readonly Random _random = new();

    public object Create(object request, ISpecimenContext context)
    {
        Guard.NotNull(context);

        if (request is not SeededRequest seededRequest)
        {
            return new NoSpecimen();
        }

        var typeRequested = seededRequest.Request as Type;
        if (typeRequested != typeof(byte[]))
        {
            return new NoSpecimen();
        }

        var length = context.Create<int>();
        var byteArray = new byte[length];
        _random.NextBytes(byteArray);
        return byteArray;
    }
}
