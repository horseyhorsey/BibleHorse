namespace BH.TelegramBot.Service
{
    public static class Messages
    {
        public const string MSG_WELCOME = @"Send me a book, chapter & verse range to return.

Must be in the format of {book},{chapter}:{verseRange}

rev,9:11-13 = Will return Revelation Chapter 9, verses 11-13
gen,6:2-5 = Will return Genesis Chapter 6, verses 2-5
gen,1:1 = Will return Genesis Chapter 1 verse 1

Use / to show list of other commands";
    }
}
