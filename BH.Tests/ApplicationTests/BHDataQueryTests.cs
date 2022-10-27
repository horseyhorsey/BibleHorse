using BH.Application.Features.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BH.Tests.ApplicationTests
{
    public abstract class BhDataQueryBase
    {
        internal ServiceProvider _provider;
        internal IMediator Mediator { get; }

        public BhDataQueryBase()
        {
            _provider = ServicesLoader.LoadServices();
            Mediator = _provider.GetRequiredService<IMediator>();
        }
    }

    /// <summary>
    /// Tests for querying data from database
    /// </summary>
    public class BHDataQueryTests : BhDataQueryBase
    {
        [Fact]
        public async Task GetBooksList()
        {
            var q = new GetBooksQuery();

            var verseResult = await Mediator.Send(q);

            Assert.NotNull(verseResult);
        }

        [Fact]
        public async Task GetBookInfo()
        {
            var q = new GetBookInfoQuery("Gen");
            var bookInfo = await Mediator.Send(q);

            Assert.NotNull(bookInfo);
            Assert.True(bookInfo.Chapters == 50);
        }

        [Theory]
        [InlineData("1 j,1:1-5")]
        [InlineData("gen,1:1-5")]
        [InlineData("gen,1:5-1")]
        [InlineData("gen,6:2")]
        public async Task GetVerses(string query)
        {
            var q = new GetVersesQuery(new VersesQueryDto
            {
                Query = query
            });

            var verseResult = await Mediator.Send(q);

            Assert.NotNull(verseResult);
        }

        [Fact]
        public async Task GetSingleVerseTests()
        {
            var q = new GetVerseQuery(new VerseQueryDto
            {
                Book = "Joh",
                Chapter = 1,
                Verse = 1
            });

            var verseResult = await Mediator.Send(q);

            Assert.NotNull(verseResult);
        }
    }
}
