using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using WebServices.DataAccess.Contracts;
using WebServices.Entities.Models.Contracts;

namespace WebServices.DataAccess
{
    public class DBContext<TEntity> : IDBContext<TEntity> where TEntity : class, IEntity
    {
        private readonly DbSet<TEntity> _items;
        private readonly ConnectionContext _connectionContext;
        public DBContext(ConnectionContext connectionContext)
        {
            _connectionContext = connectionContext;
            _items = _connectionContext.Set<TEntity>();
        }
        public TEntity Save(TEntity entity)
        {
            _items.Add(entity);
            _connectionContext.SaveChanges();
            return entity;
        }
        public IList<TEntity> GetAll()
        {
            return _items.ToList();
        }
        public void DeleteAll()
        {
            _connectionContext.Set<TEntity>().RemoveRange(_connectionContext.Set<TEntity>());
            _connectionContext.SaveChanges();
        }
    }
}
