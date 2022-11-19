namespace BH.Domain.Model
{
    public class Book : BHEntity
    {
        public string Name { get; set; }
        public string Introduction { get; set; }
        public long TranslationId { get; set; }
        public Translation Translation { get; set; }
    }
}