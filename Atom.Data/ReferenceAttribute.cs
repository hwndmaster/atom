using Genius.Atom.Infrastructure.Entities;

namespace Genius.Atom.Data;

/// <summary>
///   Marks the property as a reference object, which changes the (de-)serialization behavior to produce only the Id of the associated EntityBase.
///   NOTE: The property has to be of <seealso cref="EntityBase"/> type, otherwise this attribute is ignored.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ReferenceAttribute: Attribute
{
}
