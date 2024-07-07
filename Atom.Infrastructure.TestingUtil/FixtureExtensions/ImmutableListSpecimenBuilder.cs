using System.Collections.Immutable;
using AutoFixture.Kernel;

namespace Genius.Atom.Infrastructure.TestingUtil.FixtureExtensions;

public class ImmutableListSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        Guard.NotNull(context);

        var typeRequested = request as Type;
        if (typeRequested is null)
        {
            return new NoSpecimen();
        }

        var typeArguments = typeRequested.GetGenericArguments();
        if (typeArguments.Length == 1 && typeof(ImmutableArray<>) == typeRequested.GetGenericTypeDefinition())
        {
            dynamic list = context.Resolve(typeof(IList<>).MakeGenericType(typeArguments));
            return ImmutableArray.ToImmutableArray(list);
        }
        else if (typeArguments.Length == 1 && typeof(ImmutableList<>) == typeRequested.GetGenericTypeDefinition())
        {
            dynamic list = context.Resolve(typeof(IList<>).MakeGenericType(typeArguments));
            return ImmutableList.ToImmutableList(list);
        }

        return new NoSpecimen();
    }
}
