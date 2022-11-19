using BH.Domain.Model;
using ImageMagick;

namespace BH.Infrastructure.ImgGen
{
    public class BibleMeme : IBibleMeme
    {
        public void CreateBibleMemeToFile(Verse verse, string source, string dest)
        {
            var textToWrite = verse?.Text ?? "Insert This Text Into Image";
            textToWrite = textToWrite.Replace("<br>", Environment.NewLine);
            textToWrite += Environment.NewLine + $"{verse.Book.Name} {verse.Chapter}:{verse.VerseId}";
            // These settings will create a new caption
            // which automatically resizes the text to best
            // fit within the box.
            var settings = new MagickReadSettings
            {
                Font = "Calibri",
                FontPointsize = 22,
                TextGravity = Gravity.Northwest,
                BackgroundColor = MagickColor.FromRgba(250, 235, 215, 55),// MagickColors.AntiqueWhite,
                Height = 300, // height of text box
                Width = 600 // width of text box
            };

            using (var image = new MagickImage(source))
            {
                using (var caption = new MagickImage($"caption:{textToWrite}", settings))
                {
                    // Add the caption layer on top of the background image
                    // at position 590,450
                    image.Composite(caption, 10, 45, CompositeOperator.Over);

                    image.Write(dest);
                }
            }
        }
    }

    public interface IBibleMeme
    {
    }
}
