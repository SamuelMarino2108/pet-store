namespace Store.Domain.Common;

/// <summary>
/// Base class for entities. Ids are UUID v7: generated in the domain (no database round trip)
/// and time-ordered, which keeps B-tree indexes compact.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.CreateVersion7();
}
