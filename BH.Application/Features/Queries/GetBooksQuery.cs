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
        public GetBooksQuery()
        {
        }
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
            var query = await repository.ListAsync<Book>();
            var result = string.Join(", ", query.Select(x => x.Name));
            return result;
        }
    }  
}
