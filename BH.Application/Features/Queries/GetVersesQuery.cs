using Ardalis.Specification;
using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace BH.Application.Features.Queries
{
    /// <summary>
    /// Return verses with ranges based on a small query like-- rev,9:11-13
    /// </summary>
    public class GetVersesQuery : IRequest<IEnumerable<Verse>>
    {
        public GetVersesQuery(VersesQueryDto verseQuery)
        {
            VerseQuery = verseQuery;
        }

        public VersesQueryDto VerseQuery { get; }
    }

    public class GetVersesQueryHandler : IRequestHandler<GetVersesQuery, IEnumerable<Verse>>
    {
        private readonly IRepository repository;
        private readonly ILogger<GetVersesQuery> logger;

        public GetVersesQueryHandler(IRepository repository, ILogger<GetVersesQuery> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public async Task<IEnumerable<Verse>> Handle(GetVersesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new VersesSpecification(request.VerseQuery);
                var query = repository.ApplySpecification(spec).AsNoTracking();
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return null;
            }
        }
    }

    public class VersesSpecification : Specification<Verse>
    {
        public VersesSpecification(VersesQueryDto verseQuery)
        {
            if (!string.IsNullOrWhiteSpace(verseQuery.Query))
            {

                var split = verseQuery.Query.Split(",");
                if(split.Length == 2)
                {
                    var bookTitle = split[0];
                    var chapterVerses = split[1].Split(":");
                    if(chapterVerses.Length == 2)
                    {
                        var chapter = chapterVerses[0];
                        var verseSplit = chapterVerses[1].Split("-");
                        int.TryParse(chapter, out var chapterNo);

                        if (verseSplit.Length == 2)
                        {                            
                            int.TryParse(verseSplit[0], out var verseRangeA);
                            int.TryParse(verseSplit[1], out var verseRangeB);

                            if(verseRangeA > verseRangeB)
                            {
                                var b = verseRangeB;
                                verseRangeB = verseRangeA;
                                verseRangeA = b;
                            }

                            Query.Include(x => x.Book)
                            .Where(x => x.Book.Name.ToLower().StartsWith(bookTitle.ToLower()))
                            .Where(x => x.Chapter == chapterNo)
                            .Where(x => x.VerseId >= verseRangeA && x.VerseId <= verseRangeB);
                        }
                        else
                        {
                            int.TryParse(verseSplit[0], out var verseId);
                            Query.Include(x => x.Book)
                            .Where(x => x.Book.Name.ToLower().StartsWith(bookTitle.ToLower()))
                            .Where(x => x.Chapter == chapterNo)
                            .Where(x => x.VerseId == verseId);
                        }
                    }                    
                }
            }
        }
    }

    public class VersesQueryDto
    {
        /// <summary>
        /// query like. 1 John,1:1-5
        /// </summary>
        public string Query { get; set; }
    }
}
