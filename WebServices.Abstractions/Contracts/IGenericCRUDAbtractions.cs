using System.Collections.Generic;

namespace WebServices.Abstractions
{
    public interface IGenericCrudAbtractions<TEntity>
    {
        TEntity Save(TEntity entity);
        IList<TEntity> GetAll();
        void DeleteAll();
    }
}
