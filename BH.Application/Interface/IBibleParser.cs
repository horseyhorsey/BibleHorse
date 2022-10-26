using BH.Domain.Model;

namespace BH.Application.Interface
{
    public interface IBibleParser
    {
        List<Book> Books { get; set; }
        Verse CurrentVerse { get; set; }
        List<Verse> Verses { get; set; }
        Book AddBook(int id, string name, string intro);
        Verse AddVerse(Book book, int verseId, string contents, int chapterId);
        int TryAddVerse(Book book, string innerText, string? value, int chapterId);
    }
}