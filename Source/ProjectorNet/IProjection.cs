using System.Linq.Expressions;

namespace ProjectorNet;

#if NET7_0_OR_GREATER
public interface IProjection<TSelf, TSource, in TContext> where TSelf : IProjection<TSelf, TSource, TContext>
{
    public static abstract Expression<Func<TSource, TSelf>> GetProjectionExpression(TContext context);
}
#endif
