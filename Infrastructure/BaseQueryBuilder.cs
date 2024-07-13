using BackendServices;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Infrastructure
{

    internal class BaseQueryBuilder<TEntity> : IQueryBuilder<TEntity>
    {
        private static readonly FilterDefinitionBuilder<TEntity> filterBuilder = Builders<TEntity>.Filter;
        private FilterDefinition<TEntity> filter;
        private bool ignoreNulls;
        internal FilterDefinition<TEntity> Filter => filter;

        public BaseQueryBuilder()
        {
            filter = filterBuilder.Empty;
        }

        public BaseQueryBuilder(FilterDefinition<TEntity> filter)
        {
            this.filter = filter;
        }

        public IQueryBuilder<TEntity> IgnoreNulls(bool ignore)
        {
            ignoreNulls = ignore;
            return this;
        }

        public IQueryBuilder<TEntity> New()
            => InternalNew();

        private BaseQueryBuilder<TEntity> InternalNew()
            => new BaseQueryBuilder<TEntity>();

        public IQueryBuilder<TEntity> And<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            if (ignoreNulls && value is null)
                return this;

            filter = filterBuilder.And(filter, filterBuilder.Eq(field, value));
            return this;
        }

        public IQueryBuilder<TEntity> And<TField>(IQueryBuilder<TEntity> query)
        {
            var queryBuilder = (BaseQueryBuilder<TEntity>)query;
            filter = filterBuilder.And(filter, queryBuilder);
            return this;
        }

        public IQueryBuilder<TEntity> Eq<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            if (ignoreNulls && value is null)
                return this;

            var eq = InternalNew();
            eq.IgnoreNulls(ignoreNulls);
            
            eq.filter = filterBuilder.Eq(field, value);
            return eq;
        }

        public IQueryBuilder<TEntity> Gt<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            if (ignoreNulls && value is null)
                return this;

            var gt = InternalNew();
            gt.IgnoreNulls(ignoreNulls);
            gt.filter = filterBuilder.Gt(field, value);
            return gt;
        }

        public IQueryBuilder<TEntity> Gte<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            if (ignoreNulls && value is null)
                return this;

            var gte = InternalNew();
            gte.IgnoreNulls(ignoreNulls);
            gte.filter = filterBuilder.Gte(field, value);
            return gte;
        }

        public IQueryBuilder<TEntity> Lte<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            if (ignoreNulls && value is null)
                return this;

            var lte = InternalNew();
            lte.IgnoreNulls(ignoreNulls);
            lte.filter = filterBuilder.Lte(field, value);
            return lte;
        }

        public IQueryBuilder<TEntity> In<TField>(Expression<Func<TEntity, TField>> field, IEnumerable<TField> values)
        {
            if (ignoreNulls && values is null)
                return this;
            var _in = InternalNew();
            _in.IgnoreNulls(ignoreNulls);
            _in.filter = filterBuilder.In(field, values);
            return _in;
        }

        public IQueryBuilder<TEntity> NotIn<TField>(Expression<Func<TEntity, TField>> field, IEnumerable<TField> values)
        {
            if (ignoreNulls && values is null)
                return this;
            var nin = InternalNew();
            nin.IgnoreNulls(ignoreNulls);
            nin.filter = filterBuilder.Nin(field, values);
            return nin;
        }

        public IQueryBuilder<TEntity> Or<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            if (ignoreNulls && value is null)
                return this;

            filter = filterBuilder.Or(filter, filterBuilder.Eq(field, value));
            return this;
        }

        public IQueryBuilder<TEntity> Or<TField>(IQueryBuilder<TEntity> query)
        {
            var baseBuilder = (BaseQueryBuilder<TEntity>)query;
            filter = filterBuilder.Or(filter, baseBuilder);
            return this;
        }

        public static IQueryBuilder<TEntity> And<TField>(IQueryBuilder<TEntity> left, IQueryBuilder<TEntity> right)
        {
            var leftBuilder = (BaseQueryBuilder<TEntity>)left;
            var rightBuilder = (BaseQueryBuilder<TEntity>)right;
            var filter = filterBuilder.And(leftBuilder, rightBuilder);
            return new BaseQueryBuilder<TEntity>(filter);
        }

        public static IQueryBuilder<TEntity> Or<TField>(IQueryBuilder<TEntity> left, IQueryBuilder<TEntity> right)
        {
            var leftBuilder = (BaseQueryBuilder<TEntity>)left;
            var rightBuilder = (BaseQueryBuilder<TEntity>)right;
            var filter = filterBuilder.Or(leftBuilder, rightBuilder);
            return new BaseQueryBuilder<TEntity>(filter);
        }

        public static implicit operator FilterDefinition<TEntity>(BaseQueryBuilder<TEntity> builder)
            => builder.Filter;

        public override string ToString()
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<TEntity>();

            return Filter.Render(documentSerializer, serializerRegistry).ToString();

        }
    }

    internal class QueryBuilderProvider : IQueryBuilderProvider
    {
        public IQueryBuilder<TType> For<TType>()
            => new BaseQueryBuilder<TType>();
    }
}
