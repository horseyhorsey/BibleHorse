using BH.Application.Features.Queries;
using BH.Infrastructure.ImgGen;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.ImageGenTests
{
    public class BibleMemeTests
    {
        [Fact]
        public async Task GenerateImageFromVerseTests()
        {
            //var q = new GetBooksQuery();
            //var verseResult = await Mediator.Send(q);
            //Assert.NotNull(verseResult);

            var meme = new BibleMeme();

            meme.CreateBibleMemeToFile(new Domain.Model.Verse
            {
                Book = new Domain.Model.Book() { Name = "Psalms"},
                VerseId = 1,
                Chapter = 1,
                Text = "So, in His anger, He will speak,<br>And in His rage, He will disturb them and say:<br>"
            }, "source.png", "dest.png");
        }
    }
}
