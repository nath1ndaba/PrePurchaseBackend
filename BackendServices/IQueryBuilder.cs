using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BackendServices
{
    public interface IQueryBuilder<TEntity>
    {
        IQueryBuilder<TEntity> IgnoreNulls(bool ignore);
        IQueryBuilder<TEntity> New();
        IQueryBuilder<TEntity> Eq<TField>(Expression<Func<TEntity, TField>> field, TField value);
        IQueryBuilder<TEntity> And<TField>(Expression<Func<TEntity, TField>> field, TField value);
        IQueryBuilder<TEntity> And<TField>(IQueryBuilder<TEntity> query);
        IQueryBuilder<TEntity> Gt<TField>(Expression<Func<TEntity, TField>> field, TField value);
        IQueryBuilder<TEntity> Gte<TField>(Expression<Func<TEntity, TField>> field, TField value);
        IQueryBuilder<TEntity> Lte<TField>(Expression<Func<TEntity, TField>> field, TField value);
        IQueryBuilder<TEntity> In<TField>(Expression<Func<TEntity, TField>> field, IEnumerable<TField> values);
        IQueryBuilder<TEntity> NotIn<TField>(Expression<Func<TEntity, TField>> field, IEnumerable<TField> values);
        IQueryBuilder<TEntity> Or<TField>(IQueryBuilder<TEntity> query);
        IQueryBuilder<TEntity> Or<TField>(Expression<Func<TEntity, TField>> field, TField value);

    }

    public interface IQueryBuilderProvider
    {
        IQueryBuilder<TType> For<TType>();
    }
}
