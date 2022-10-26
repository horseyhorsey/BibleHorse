using BH.Application.Interface;
using BH.Domain.Model;
using HtmlAgilityPack;

namespace BH.Infrastructure.Parsers
{
    /// <summary>
    /// Our Bible text includes hyperlinks to other websites (e.g. Wikipedia), and to our translator notes and commentaries; you may remove these links if you wish, but we would prefer that you leave them intact if at all possible. Also, a link to 2001translation.org would be very much appreciated.
    /// Download it as a plain HTML file:
    /// https://downloads.2001translation.org/html/2001-text.html <para/>
    /// Class to parse a 2001 html file for books, chapters & verses
    /// </summary>
    public class Parser_2001 : IBibleParser2011
    {
        public List<Book> Books { get; set; } = new List<Book>();
        public List<Verse> Verses { get; set; } = new List<Verse>();
        public Book CurrentBook { get; set; } = new Book();
        public int CurrentChapter { get; set; }
        public Verse CurrentVerse { get; set; } = new Verse();

        public Book AddBook(int id, string name, string intro)
        {
            Books.Add(new Book() { Id = id, Name = name, Introduction = intro });
            return Books.Last();
        }

        public bool AddTabOrSpace(HtmlNode? element)
        {
            if (CurrentVerse.Text.TrimEnd().EndsWith("<br>"))
                return false;

            if (element.HasClass("tabbed"))
            {
                CurrentVerse.AppendText("<br>");
                return true;
            }
            else if (element.HasClass("smspace") || element.HasClass("br") || element.HasClass("lgspace"))
            {
                CurrentVerse.AppendText("<br>");
                return true;
            }

            return false;
        }

        public Verse AddVerse(Book book, int verseId, string contents, int chapterId)
        {
            var v = new Verse() { Book = book, BookId = book.Id, Text = contents, VerseId = verseId, Chapter = chapterId };
            Verses.Add(v);
            return v;
        }

        /// <summary>
        /// Converts a 2001 html document into Books / Verses
        /// </summary>
        /// <param name="htmlDocument"></param>
        public void ConvertHtmlDocument(string htmlDocument = "2001-text.html", string translationName = "2001")
        {
            //load document local and select the main body to node
            var web = new HtmlDocument();
            var url = htmlDocument;
            web.Load(url);
            var node = web.DocumentNode.SelectSingleNode("//body");

            //get all books in bible, 66 books in bible usually but includes Daniel old greek
            var books = node.ChildNodes.Where(x => x.Name == "div");
            int bookId = 1;
            var translation = new Translation { Name = translationName };

            foreach (var divBook in books)
            {
                foreach (var book in divBook.ChildNodes.Where(x => x.Name != "#text"))
                {
                    if (book.Name == "h1") //header 1 should be the Book title
                    {
                        //add a new book to the list and set to current book to work from
                        var newBook = new Book() { Id = bookId, Translation = translation };
                        Books.Add(newBook);
                        CurrentBook = newBook;
                        CurrentBook.Name = book.InnerText;
                        bookId++;
                    }
                    else if (book.Id.Contains("introduction")) //Books have introduction texts
                    {
                        var nodes = book.ChildNodes.Where(x => x.Name != "#text");
                        var introNode = nodes.ElementAt(1);
                        CurrentBook.Introduction = introNode.InnerHtml;
                    }
                    else
                    {
                        var nodes = book.ChildNodes.Where(x => x.Name != "#text");
                        var paragraph = nodes.ElementAt(1);
                        var currentVerseText = string.Empty;

                        if (book.Id.Contains("chapter"))
                        {
                            int chapterId = 0;
                            int.TryParse(book.ChildNodes.Where(x => x.Name == "h2")?.First()?.InnerText?.Replace("Chapter ", ""), out chapterId);
                            CurrentChapter = chapterId;

                            //if (Parser.CurrentChapter > 1) break; //temp stop loop single chapter
                        }

                        foreach (var item in paragraph.ChildNodes)
                        {
                            if (item.HasClass("editor"))
                                continue;

                            //most lines are <span/> with nested elements
                            if (item.Name == "span" && !item.HasClass("editor"))
                            {
                                ParseHtmlElement(item);
                                continue;

                                if (item.ChildNodes.Count > 0)
                                {
                                    AddTabOrSpace(item);

                                    foreach (var spanItem in item.ChildNodes)
                                    {
                                        if (spanItem.Name == "span" || spanItem.Name == "b")
                                        {
                                            if (spanItem.ChildNodes.Count > 1)
                                            {
                                                foreach (var supIItem in spanItem.ChildNodes)
                                                {
                                                    if (supIItem.Name == "sup" && supIItem.HasAttributes)
                                                    {
                                                        TryAddVerse(CurrentBook, supIItem.InnerText, supIItem.Attributes["id"]?.Value, CurrentChapter);
                                                    }
                                                    else if (supIItem.Name == "i")
                                                    {
                                                        CurrentVerse.AppendText(supIItem.OuterHtml);
                                                    }
                                                    else if (supIItem.Name == "span")
                                                    {
                                                        AddTabOrSpace(spanItem);
                                                    }
                                                    else if (item.ChildNodes[0].Name == "i")
                                                    {
                                                        CurrentVerse.AppendText(item.OuterHtml);
                                                    }
                                                    else
                                                    {
                                                        if (AddTabOrSpace(spanItem)) { }
                                                        else { CurrentVerse.AppendText(supIItem.OuterHtml); }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (AddTabOrSpace(spanItem)) { }
                                                else
                                                {
                                                    CurrentVerse.AppendText(spanItem.InnerHtml);
                                                }

                                            }
                                        }
                                        else if (spanItem.Name == "sup" && spanItem.HasAttributes)
                                        {
                                            TryAddVerse(CurrentBook, spanItem.InnerText, spanItem.Attributes["id"]?.Value, CurrentChapter);
                                        }
                                        else if (spanItem.Name == "a") { CurrentVerse.AppendText(spanItem.OuterHtml); }
                                        else
                                        {
                                            if (spanItem.ChildNodes.Count > 0)
                                            {
                                                foreach (var si in spanItem.ChildNodes)
                                                {
                                                    if (si.ChildNodes.Count > 0)
                                                    {
                                                        foreach (var ii in si.ChildNodes)
                                                        {
                                                            if (ii.Name == "sup" && ii.HasAttributes)
                                                            {
                                                                TryAddVerse(CurrentBook, si.InnerText, si.Attributes["id"]?.Value, CurrentChapter);
                                                            }
                                                            else
                                                            {
                                                                CurrentVerse.AppendText(ii.OuterHtml);
                                                            }
                                                        }
                                                    }
                                                    else if (si.Name == "sup" && si.HasAttributes)
                                                    {
                                                        TryAddVerse(CurrentBook, si.InnerText, si.Attributes["id"]?.Value, CurrentChapter);
                                                    }
                                                    else if (si.Name == "i")
                                                    {
                                                        CurrentVerse.AppendText(spanItem.OuterHtml);
                                                    }
                                                    else
                                                    {
                                                        CurrentVerse.AppendText(si.OuterHtml);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ParseHtmlElement(item);
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    AddTabOrSpace(item);
                                }
                            }
                            else
                            {
                                ParseHtmlElement(item);
                            }
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// Parses a verse id from "sup" html elements. These are sometimes nested in span elements.
        /// </summary>
        /// <param name="innerText"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int GetSupVerseId(string innerText, string? value)
        {
            int result = 0;
            if (value != null && value.StartsWith("_"))
            {
                int.TryParse(innerText, out result);
            }

            return result;
        }

        public void ParseHtmlElement(HtmlNode? item)
        {
            if (item.Name == "#text")
            {
                CurrentVerse.AppendText(item.InnerText);
            }
            else if (item.Name == "sup" && item.HasAttributes)
            {
                TryAddVerse(CurrentBook, item.InnerText, item.Attributes["id"]?.Value, CurrentChapter);
            }
            else if (item.Name == "i")
            {
                CurrentVerse.AppendText(item.OuterHtml);
            }
            else if (item.Name == "span" || item.Name == "b")
            {
                AddTabOrSpace(item);
                if (item.ChildNodes.Count > 0)
                {
                    foreach (var child in item.ChildNodes)
                    {
                        ParseHtmlElement(child);
                    }
                }
                else
                {
                    CurrentVerse.AppendText(item.InnerHtml);
                }
            }
            else
            {
                if (item.Name == "a")
                {
                    CurrentVerse.AppendText(item.OuterHtml);
                }
                else if (item.Name == "br")
                {

                }
                else
                {
                    CurrentVerse.AppendText(item.OuterHtml);
                }
            }
        }

        /// <summary>
        /// Creates a new verse if verse number found
        /// </summary>
        /// <param name="book"></param>
        /// <param name="innerText"></param>
        /// <param name="value"></param>
        /// <param name="currentVerseText"></param>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        public int TryAddVerse(Book book, string innerText, string? value, int chapterId)
        {
            int result = GetSupVerseId(innerText, value);
            if (result > 0)
            {
                CurrentVerse = AddVerse(book, result, string.Empty, chapterId);
            }

            return result;
        }
    }
}
