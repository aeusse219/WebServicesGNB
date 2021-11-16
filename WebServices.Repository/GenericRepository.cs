using System.Collections.Generic;
using WebServices.Abstractions;
using WebServices.DataAccess.Contracts;
using WebServices.Entities.Models.Contracts;
using WebServices.Repository.Contracts;

namespace WebServices.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : IEntity
    {
        private readonly IDBContext<TEntity> _DBContext;
        public GenericRepository(IDBContext<TEntity> DBContext)
        {
            _DBContext = DBContext;
        }
        public TEntity Save(TEntity entity)
        {
            return _DBContext.Save(entity);
        }
        IList<TEntity> IGenericCrudAbtractions<TEntity>.GetAll()
        {
            return _DBContext.GetAll();
        }
        public void DeleteAll()
        {
            _DBContext.DeleteAll();
        }
    }
}
