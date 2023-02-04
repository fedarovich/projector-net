using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectorNet;

public static class QueryableExtensions
{
    public static Projector<TEntity, object?> Project<TEntity>(this IQueryable<TEntity> query)
    {
        return new Projector<TEntity, object?>(query, null);
    }

    public static Projector<TEntity, TContext> Project<TEntity, TContext>(this IQueryable<TEntity> query, TContext context)
    {
        return new Projector<TEntity, TContext>(query, context);
    }
}
