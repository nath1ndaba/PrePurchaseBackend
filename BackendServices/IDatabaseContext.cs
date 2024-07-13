using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BackendServices
{
    public interface IDatabaseContext
    {
        Task Initialize();
        Task Insert<T>(T data, string collectionName = null);
        Task Insert<T>(IEnumerable<T> data, string collectionName = null);
        Task<T> Update<T>(string id, T data, string collectionName = null);
        Task<T> Upsert<T>(string id, T data, string collectionName = null);
        Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> predicate, string collectionName = null, int skip = 0, int limit = 10_000);
        Task<T> FindOne<T>(Expression<Func<T, bool>> predicate, string collectionName = null);
        Task<T> FindById<T>(string id, string collectionName = null);
    }
}
