using Ardalis.Specification;
using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;

namespace BH.Application.Features.Queries
{
    /// <summary>
    /// Resturns list of books as CSV
    /// </summary>
    public class GetBooksQuery : IRequest<string>
    {
        /// <summary>
        /// Defaults to transaltion for 2001, leaving open for more translations
        /// </summary>
        /// <param name="translationId"></param>
        public GetBooksQuery(long translationId = 1)
        {
            TranslationId = translationId;
        }

        public long TranslationId { get; }
    }

    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, string>
    {
        private readonly IRepository repository;

        public GetBooksQueryHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<string> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            var query = await repository.ListAsync(new BookQuerySpec(request.TranslationId));
            var result = string.Join(", ", query.Select(x => x.Name));
            return result;
        }
    }  

    /// <summary>
    /// Spec for querying book by translation
    /// </summary>
    public class BookQuerySpec : Specification<Book>
    {
        public BookQuerySpec(long translationId = 1)
        {
            Query.Where(x => x.TranslationId == translationId);
        }
    }
}
