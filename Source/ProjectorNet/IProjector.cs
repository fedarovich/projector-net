namespace ProjectorNet;

public interface IProjector<TSource, out TContext>
{
    IQueryable<TSource> Query { get; }

    TContext Context { get; }

#if NET7_0_OR_GREATER
    IQueryable<TProjection> To<TProjection>() where TProjection : IProjection<TProjection, TSource, TContext>;
#endif
}
