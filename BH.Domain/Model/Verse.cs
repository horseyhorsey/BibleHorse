namespace BH.Domain.Model
{
    public class Verse : BHEntity
    {
        public int BookId { get; set; }
        public int VerseId { get; set; }
        public int Chapter { get; set; }
        public string Text { get; set; }
        public Book Book { get; set; }
        public void AppendText(string text) => Text += text;
    }
}