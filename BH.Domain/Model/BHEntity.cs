namespace BH.Domain.Model
{
    public abstract class BHEntity : IAggregateRoot
    {
        public long Id { get; set; }
    }    
}
