using BH.Infrastructure.Parsers;

namespace BH.Tests
{
    public partial class ParserTests2001
    {
        internal Parser_2001 Parser { get; set; } = new Parser_2001();

        /// <summary>
        /// Converts 2001 html to models ready for inserting to database
        /// </summary>
        [Fact]
        public void ParseAllBooks_2001()
{
            Parser.ConvertHtmlDocument();

            //book count should be 67, extra daniel
            Assert.Equal(67, Parser.Books.Count());

            //chapters Genesis 1 = 31
            var chapters = Parser.Verses.Where(x => x.BookId == 1 && x.Chapter == 1)?.Count();
            Assert.True(chapters == 31);

            //chapters 1 Peter 1 = 25
            chapters = Parser.Verses.Where(x => x.BookId == 61 && x.Chapter == 1)?.Count();
            Assert.True(chapters == 25);

            //Ezra missing 6:5 to equal 21 verses
            var verses = Parser.Verses.Where(x => x.BookId == 15 && x.Chapter == 6);
            Assert.True(verses.Count() == 21);
        }
    }
}