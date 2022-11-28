using Ardalis.Specification;
using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;

namespace BH.Application.Features.Queries
{
    public class SearchVerseQuery : IRequest<SearchVerseResultVm>
    {
        public SearchVerseQuery(string searchTerm, int pageLimit = 10, long translationId = 1)
        {
            SearchTerm = searchTerm;
            PageLimit = pageLimit;
            TranslationId = translationId;
        }

        public string SearchTerm { get; }
        public int PageLimit { get; }
        public long TranslationId { get; }
    }

    public class SearchVerseQueryHandler : IRequestHandler<SearchVerseQuery, SearchVerseResultVm>
    {
        private readonly IRepository repository;

        public SearchVerseQueryHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<SearchVerseResultVm> Handle(SearchVerseQuery request, CancellationToken cancellationToken)
        {
            var resultVm = new SearchVerseResultVm();
            var searchTerms = request.SearchTerm.Split(":");

            SearchVerseSpec searchSpec = null;
            int pageNumber = 1;
            string searchTerm = string.Empty;
            string bookTitle = string.Empty;

            //search with a book title or just standard
            if (searchTerms.Length == 2)
            {
                int.TryParse(searchTerms[1], out var page);
                if(page > 0)
                {
                    pageNumber = page;
                    searchTerm = searchTerms[0];

                    resultVm.SearchTerm = request.SearchTerm; //add page number to search term
                    searchSpec = new SearchVerseSpec(searchTerm, bookTitle);
                }
                else
                {
                    searchTerm = searchTerms[1];
                    bookTitle = searchTerms[0];

                    resultVm.SearchTerm = request.SearchTerm + $":{pageNumber}"; //add page number to search term
                    searchSpec = new SearchVerseSpec(searchTerms[1], searchTerms[0]);
                }                
            }
            else if (searchTerms.Length == 3)
            {
                searchTerm = searchTerms[1];
                bookTitle = searchTerms[0];                
                resultVm.SearchTerm = request.SearchTerm; //add page number to search term
                searchSpec = new SearchVerseSpec(searchTerm, bookTitle);
                int.TryParse(searchTerms[2], out pageNumber);
            }
            else
            {
                searchTerm = request.SearchTerm;
                resultVm.SearchTerm = request.SearchTerm + $":{pageNumber}"; //add page number to search term
                searchSpec = new SearchVerseSpec(searchTerm, bookTitle);   
            }

            if (searchTerm.Length < 3)
                throw new ArgumentException("Search term must be longer than 3 chars");

            //Get the total results first
            resultVm.ResultTotal = await repository.CountAsync(searchSpec);            
            resultVm.Pages = (int)Math.Round((decimal)resultVm.ResultTotal / (decimal)request.PageLimit, MidpointRounding.ToPositiveInfinity);

            //Run method again but with paging
            pageNumber = pageNumber == 0 ? 1 : pageNumber;

            if (pageNumber > resultVm.Pages)
                pageNumber = resultVm.Pages;

            resultVm.Page = pageNumber;
            searchSpec = new SearchVerseSpec(searchTerm, bookTitle, request.PageLimit, pageNumber, true, request.TranslationId);
            resultVm.Verses = await repository.ListAsync(searchSpec);

            return resultVm;
        }
    }

    public class SearchVerseSpec : Specification<Verse>
    {
        public SearchVerseSpec(string searchTerm, string bookTitle, int pageLimit = 10, int currentPage = 1, bool pageQuery = false, long translationId = 1)
        {
            Query.Include(x => x.Book);
            Query.Where(x => x.Book.TranslationId == translationId);

            if (!string.IsNullOrWhiteSpace(bookTitle))
            {
                if(bookTitle == "ot")
                {
                    Query.Where(x => x.Book.Id < 41);
                }
                else if (bookTitle == "nt")
                {
                    Query.Where(x => x.Book.Id > 40);
                }
                else
                {
                    Query.Where(x => x.Book.Name.ToLower().Contains(bookTitle.ToLower()));
                }                
            }

            searchTerm = searchTerm.Insert(0, "%");
            searchTerm = searchTerm.Insert(searchTerm.Length, "%");
            Query.Search(x => x.Text, searchTerm);

            if (pageQuery)
                PageQuery(pageLimit, currentPage);
        }

        void PageQuery(int pageLimit = 10, int currentPage = 1)
        {
            Query.OrderBy(x => x.BookId).ThenBy(x => x.Chapter).ThenBy(x => x.VerseId);
            
            if(currentPage > 1)
            {
                var skipIndex = (currentPage-1) * pageLimit;

                Query.Skip(skipIndex);
            }            
            Query.Take(pageLimit);
        }
    }

    public class SearchVerseResultVm
    {
        public int ResultTotal { get; set; }
        public string SearchTerm { get; set; }
        public IEnumerable<Verse> Verses { get; set; }
        public int Page { get; set; }
        public int Pages { get; set; }
        public string NextPageQuery { get; internal set; }
    }
}
