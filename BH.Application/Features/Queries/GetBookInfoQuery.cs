using Ardalis.Specification;
using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BH.Application.Features.Queries
{
    /// <summary>
    /// Resturns information about a given book
    /// </summary>
    public class GetBookInfoQuery : IRequest<BookInfoVm>
    {
        public GetBookInfoQuery(string bookTitle, long translationId = 1)
        {
            BookTitle = bookTitle;
            TranslationId = translationId;
        }

        public string BookTitle { get; }
        public long TranslationId { get; }
    }

    public class GetBookInfoQueryHandler : IRequestHandler<GetBookInfoQuery, BookInfoVm>
    {
        private readonly IRepository repository;

        public GetBookInfoQueryHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<BookInfoVm> Handle(GetBookInfoQuery request, CancellationToken cancellationToken)
        {
            var query = repository.ApplySpecification(new BookInfoSpecification(request.BookTitle));
            var chapterCount = await query.Select(x => new { x.Chapter}).Distinct().CountAsync();
            var verseCount = await query.Select(x => new { x.VerseId }).CountAsync();
            var book = await query.FirstOrDefaultAsync(cancellationToken);

            return new BookInfoVm { Chapters = chapterCount, Verses = verseCount, Book = book.Book};
        }
    }

    public class BookInfoVm
    {
        public Book? Book { get; set; }
        public int Chapters { get; set; }
        public int Verses { get; set; }        
    }

    public class BookInfoSpecification : Specification<Verse>
    {
        public BookInfoSpecification(string bookTitle, long translationId = 1)
        {
            if (!string.IsNullOrWhiteSpace(bookTitle))
            {
                Query.Include(x => x.Book);
                Query.Where(x => x.Book.Name.ToLower().StartsWith(bookTitle.ToLower()) && x.Book.TranslationId == translationId);
            }
        }
    }
}
