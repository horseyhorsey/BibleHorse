using HtmlAgilityPack;

namespace BH.Application.Interface
{
    public interface IBibleParser2011 : IBibleParser
    {
        bool AddTabOrSpace(HtmlNode? spanItem);
        int GetSupVerseId(string innerText, string? value);
        void ParseHtmlElement(HtmlNode? item);
    }
}