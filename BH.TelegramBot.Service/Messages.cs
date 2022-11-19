namespace BH.TelegramBot.Service
{
    public static class Messages
    {
        public const string MSG_WELCOME = @"Send me a book, chapter & verse range to return.

Must be in the format of {book},{chapter}:{verseRange}

<code>rev,9:11-13</code> = Will return Revelation Chapter 9, verses 11-13
<code>gen,6:2-5</code> = Will return Genesis Chapter 6, verses 2-5
<code>gen,1:1</code> = Will return Genesis Chapter 1 verse 1

Use / to show list of other commands";
    }
}
