namespace BH.Domain.Model
{
    public abstract class BHEntity : IAggregateRoot
    {
        public int Id { get; set; }
    }    
}
