using Ardalis.Specification;
using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BH.Application.Features.Queries
{
    public class GetVerseQuery : IRequest<Verse>
    {
        public GetVerseQuery(VerseQueryDto verseQuery)
        {
            VerseQuery = verseQuery;
        }

        public VerseQueryDto VerseQuery { get; }
    }

    public class GetVerseQueryHandler : IRequestHandler<GetVerseQuery, Verse>
    {
        private readonly IRepository repository;

        public GetVerseQueryHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Verse> Handle(GetVerseQuery request, CancellationToken cancellationToken)
        {                       
            var spec = new VerseSpecification(request.VerseQuery);
            var query = repository.ApplySpecification(spec).AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
    }

    public class VerseSpecification : Specification<Verse>
    {
        public VerseSpecification(VerseQueryDto verseQuery)
        {
            if (!string.IsNullOrWhiteSpace(verseQuery.Book))
            {
                Query.Include(x => x.Book)
                    .Where(x => x.Book.Name.ToLower().StartsWith(verseQuery.Book.ToLower()))
                    .Where(x => x.Chapter == verseQuery.Chapter)
                    .Where(x => x.VerseId == verseQuery.Verse);
            }
        }
    }

    public class VerseQueryDto
    {
        public string Book { get; set; }
        public int Verse { get; set; }
        public int Chapter { get; set; }
    }
}
