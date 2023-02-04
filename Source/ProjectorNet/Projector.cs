namespace ProjectorNet;

public sealed class Projector<TSource, TContext> : IProjector<TSource, TContext>
{
    public Projector(IQueryable<TSource> query, TContext context)
    {
        ArgumentNullException.ThrowIfNull(query);
        Query = query;
        Context = context;
    }

    public IQueryable<TSource> Query { get; }

    public TContext Context { get; }

    public IQueryable<TProjection> To<TProjection>() where TProjection : IProjection<TProjection, TSource, TContext>
    {
        return Query.Select(TProjection.GetProjectionExpression(Context));
    }
}
