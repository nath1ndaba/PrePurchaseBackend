using BackendServices.Models;
using MongoDB.Bson;
using PrePurchase.Models;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BackendServices
{
    public interface IRepository<T> where T : new()
    {
        Task DeleteCollection();
        Task Insert(T data);
        Task Insert(IEnumerable<T> data);
        Task<T> Update(string id, T data);
        Task<(bool updated, long count)> Update(IEnumerable<KeyValuePair<IQueryBuilder<T>, IUpdateBuilder<T>>> updates);
        Task<T> Upsert(string id, T data);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate, int skip = 0, int limit = 10_000);
        Task<IEnumerable<T>> Find(IQueryBuilder<T> queryBuilder, int skip = 0, int limit = 10_000);
        Task<T> FindOne(Expression<Func<T, bool>> predicate);
        Task<T> FindOne(IQueryBuilder<T> queryBuilder);
        Task<T> FindById(string id);
        Task<long> DeleteById(string id);
        Task<long> DeleteOne(Expression<Func<T, bool>> predicate);
        Task<long> DeleteOne(IQueryBuilder<T> queryBuilder);
        IQueryable<T> AsQueryable();

        /*        Task<IEnumerable<ObjectId>> GetTopNearbyShops(ResidentLocation residentLocation, int topN);
        */

    }
}
