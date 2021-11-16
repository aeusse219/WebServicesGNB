using WebServices.Abstractions;

namespace WebServices.DataAccess.Contracts
{
    public interface IDBContext<TEntity> : IGenericCrudAbtractions<TEntity>
    {

    }
}
