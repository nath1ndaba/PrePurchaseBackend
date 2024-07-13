using BackendServices;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Infrastructure
{
    internal class BaseUpdateBuilder<TEntity> : IUpdateBuilder<TEntity>
    {
        private readonly UpdateDefinitionBuilder<TEntity> updateBuilder;
        private UpdateDefinition<TEntity> update;
        internal UpdateDefinition<TEntity> Update => update;
        public BaseUpdateBuilder()
        {
            updateBuilder = Builders<TEntity>.Update;
        }

        public IUpdateBuilder<TEntity> Set<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            var _update = updateBuilder.Set(field, value);
            UpdateUpdateDefinition(_update);
            return this;
        }

        public IUpdateBuilder<TEntity> AddToSet<TField>(Expression<Func<TEntity, IEnumerable<TField>>> field, TField value)
        {
            var _update = updateBuilder.AddToSet(field, value);
            UpdateUpdateDefinition(_update);
            return this;
        }

        public IUpdateBuilder<TEntity> AddToSetEach<TField>(Expression<Func<TEntity, IEnumerable<TField>>> field, IEnumerable<TField> values)
        {
            var _update = updateBuilder.AddToSetEach(field, values);
            UpdateUpdateDefinition(_update);

            return this;
        }

        private void UpdateUpdateDefinition(UpdateDefinition<TEntity> updateDefinition)
        {
            if (update is null)
                update = updateDefinition;
            else
                update = updateBuilder.Combine(update, updateDefinition);
        }

        public static explicit operator UpdateDefinition<TEntity>(BaseUpdateBuilder<TEntity> builder)
            => builder.Update;
    }

    internal class UpdateBuilderProvider : IUpdateBuilderProvider
    {
        public IUpdateBuilder<TType> For<TType>()
            => new BaseUpdateBuilder<TType>();
    }
}
