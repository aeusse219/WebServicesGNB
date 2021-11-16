using System.Text.Json.Serialization;
using WebServices.Entities.Models.Contracts;

namespace WebServices.Entities.Models
{
    public abstract class Entity : IEntity
    {
        [JsonIgnore]
        public int Id { get; set; }
    }
}
