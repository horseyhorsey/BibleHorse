using BH.Application.Features.Commands;
using BH.Application.Features.Queries;
using BH.Application.Interface;
using BH.Domain.Model;
using BH.Domain.Sites;
using MediatR;

namespace BH.Infrastructure.Services
{
    public class BibleService : IBibleService
    {
        private readonly IMediator mediator;

        public BibleService(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<string> GetAllBooks(long translationId = 1)
        {
            var cmd = new GetBooksQuery(translationId);
            return await mediator.Send(cmd);
        }

        public async Task<string> GetBookInformation(string bookTitle, long translationId = 1)
        {
            var cmd = new GetBookInfoQuery(bookTitle);
            try
            {
                var bookInfo = await mediator.Send(cmd);
                if (bookInfo?.Book != null)
                {
                    string bookSiteLink = SiteHelper.GetSiteLink_2001(bookInfo.Book.Name);
                    return $@"<b>{bookInfo.Book.Name}</b> (<a href='{bookSiteLink}'>2001</a>){Environment.NewLine}Chapters: {bookInfo.Chapters}{Environment.NewLine}Verses: {bookInfo.Verses}{Environment.NewLine}{bookInfo.Book.Introduction
                        .Replace("<span", "<code").Replace("</span>", "</code>")
                        .Replace("<sup", "<code").Replace("</sup>", "</code>")}";
                }
                else return $"Couldn't find any title information for book {bookTitle}";
            }
            catch
            {
                return $"Couldn't find any title information for book \"{bookTitle}\"";
            }
        }

        public async Task<string> GetVerses(long? userId, string query, long translationId = 1)
        {
            var cmd = new GetVersesQuery(new VersesQueryDto() { Query = query });
            try
            {
                var verses = await mediator.Send(cmd);
                if (verses != null && verses.Count() > 0)
                {
                    //get the user to adjust names
                    var userCmd = new GetOrAddUserCommand(userId.Value);
                    var user = await mediator.Send(userCmd);

                    //get first verse and get the book title
                    var firstVerse = verses.ElementAt(0);
                    var title = firstVerse.Book.Name;
                    //include a link back to 2001 from book name, chapter verse
                    string link2001 = SiteHelper.GetSiteLink_2001(firstVerse.Book.Name) + $"#_{firstVerse.Chapter}:{firstVerse.VerseId}";
                    string linkBibleHub = SiteHelper.GetSiteLink_BibleHub(firstVerse.Book.Name) + $"{firstVerse.Chapter}-{firstVerse.VerseId}.htm";
                    string linkGateway = SiteHelper.GetSiteLink_BibleGateway(firstVerse.Book.Name) + $"{firstVerse.Chapter}:{firstVerse.VerseId}";

                    var messageResult = title + $" <a href='{link2001}'>(2001)</a> <a href='{linkBibleHub}'>(Hub)</a> <a href='{linkGateway}'>(Gate)</a>" + Environment.NewLine;

                    foreach (var verse in verses)
                    {
                        messageResult += $"{verse.Chapter}:{verse.VerseId} {ReplaceNamesForUser(user, verse.Text)}{Environment.NewLine}";
                    }

                    return messageResult;
                }
                else
                {
                    return $"Couldn't find any verses for query = {query}";
                }
            }
            catch (Exception ex)
            {
                return $"Error for query {query}. {ex.Message}";
            }
        }

        #region Support Methods
        private string ReplaceNamesForUser(User user, string verseText)
        {
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.DevineName))
                {
                    verseText = verseText.Replace("Jehovah", user.DevineName);
                }
                if (!string.IsNullOrWhiteSpace(user.GodsSon))
                {
                    verseText = verseText.Replace("Jesus", user.GodsSon);
                }
                if (!string.IsNullOrWhiteSpace(user.Anointed))
                {
                    verseText = verseText.Replace("Anointed One", user.Anointed, StringComparison.Ordinal);
                }

                //TODO: Fix database import, Jeremiah Span tags are included when shouldn't be
                verseText = verseText.Replace("<span", "<b");
                verseText = verseText.Replace("/span", "/b");
            }

            verseText = verseText.Replace("<br>", Environment.NewLine);
            return verseText;
        }
        #endregion
    }
}
