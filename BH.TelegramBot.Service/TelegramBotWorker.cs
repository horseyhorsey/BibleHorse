using Telegram.Bot.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using BH.Application.Interface;
using MediatR;
using BH.Application.Features.Queries;
using Update = Telegram.Bot.Types.Update;
using BH.Domain.Sites;
using BH.Application.Features.Commands;
using BH.Domain.Model;
using Telegram.Bot.Types.ReplyMarkups;

namespace BH.TelegramBot.Service
{
    public class TelegramBotWorker : BackgroundService
    {
        private readonly IServiceProvider services;
        private readonly IConfiguration configuration;
        private readonly ILogger<TelegramBotWorker> _logger;
        private string _books;

        public TelegramBotWorker(IServiceProvider services, IConfiguration configuration, ILogger<TelegramBotWorker> logger)
        {
            this.services = services;
            this.configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //get all books and store local
            _books = await GetAllBooks();

            var botClient = new TelegramBotClient(configuration["TG_TOKEN"]);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(60000, stoppingToken);
            }
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            var userId = message.From.Id;
            _logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");

            if (message.ReplyToMessage != null)
            {
                if (message.ReplyToMessage.Text.Contains("Set name for"))
                {
                    using (var scope = services.CreateScope())
                    {
                        try
                        {
                            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                            var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
                            var name = message.Text.Length > 15 ? message.Text.Substring(0, 15) : message.Text;
                            UpdateUserOptionCommand cmd = new UpdateUserOptionCommand(new UpdateUserOptionDto
                            {
                                UserId = userId
                            });

                            if (message.ReplyToMessage.Text.Contains("devine")) { cmd.UserOptionDto.DevineName = name; }
                            else if (message.ReplyToMessage.Text.Contains("gods")) { cmd.UserOptionDto.GodsSon = name; }
                            else if (message.ReplyToMessage.Text.Contains("Anointed")) { cmd.UserOptionDto.Anointed = name; }
                            else { return; }

                            var result = await mediator.Send(cmd);
                            if (result)
                            {
                                await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Name has been updated to {name}",
                                parseMode: ParseMode.Html, disableWebPagePreview: true,
                                cancellationToken: cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {


                        }
                    }
                }

                return;
            }

            //process a command message
            if (message.Text.StartsWith("/"))
            {
                if (message.Text == "/start")
                {
                    //var buttons = GetStartupReplyMarkup();
                    //var markup = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
                    //await botClient.SendTextMessageAsync(
                    //    text: "Choose an option",
                    //    chatId:chatId,
                    //    disableNotification:true,
                    //    disableWebPagePreview:true,
                    //    replyMarkup: markup,                        
                    //    cancellationToken: cancellationToken);

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: Messages.MSG_WELCOME,
                    parseMode: ParseMode.Html, disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }
                else if (message.Text == "/settings")
                {
                    var buttons = GetSettingsReplyMarkup();
                    var markup = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Settings",
                    replyMarkup: markup,
                    disableNotification: true,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }
                else if (message.Text.StartsWith("/Books"))
                {
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: _books,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }
                else if (message.Text.StartsWith("/info"))
                {
                    var cmd = message.Text.Replace("/info", "").Trim();
                    var bookInfo = await GetBookInformation(cmd);
                    if (bookInfo != null)
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: bookInfo,
                        disableNotification: true,
                        parseMode: ParseMode.Html, disableWebPagePreview: true,
                        cancellationToken: cancellationToken);
                    }
                }
                else if (message.Text.StartsWith("/set_devine_name"))
                {
                    var replyMk = new ForceReplyMarkup() { };
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        disableNotification: true,
                        text: "Set name for devine",
                        replyMarkup: replyMk
                        );
                }
                else if (message.Text.StartsWith("/set_gods_son"))
                {
                    var replyMk = new ForceReplyMarkup() { };
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        disableNotification: true,
                        text: "Set name for gods son",
                        replyMarkup: replyMk
                        );
                }
                else if (message.Text.StartsWith("/set_anointed_one"))
                {
                    var replyMk = new ForceReplyMarkup() { };
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        disableNotification: true,
                        text: "Set name for Anointed One",
                        replyMarkup: replyMk
                        );
                }
            }
            //process standard
            else if (message.Text.Length > 5)
            {
                try
                {
                    var result = await GetVerses(userId, message.Text);

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    disableNotification: true,
                    text: result,
                    parseMode: ParseMode.Html, disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    disableNotification: true,
                    text: $"Couldn't find any verses for query = {message.Text}",
                    parseMode: ParseMode.Html, disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }
            }
        }

        private async Task<string> GetVerses(long? userId, string query)
        {
            using (var scope = services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
                var cmd = new GetVersesQuery(new VersesQueryDto() { Query = query });
                var verses = await mediator.Send(cmd);

                if (verses != null && verses.Count() > 0)
                {
                    //get the user to adjust names
                    var userCmd = new GetOrAddUserCommand(userId.Value);
                    var user = await mediator.Send(userCmd);

                    //get first verse and get the book title
                    var firstVerse = verses.ElementAt(0);
                    var title = firstVerse.Book.Name;
                    //include a link back to 2001 from book name, chapter verse
                    string link2001 = SiteHelper.GetSiteLink_2001(firstVerse.Book.Name) + $"#_{firstVerse.Chapter}:{firstVerse.VerseId}";
                    string linkBibleHub = SiteHelper.GetSiteLink_BibleHub(firstVerse.Book.Name) + $"{firstVerse.Chapter}-{firstVerse.VerseId}.htm";
                    string linkGateway = SiteHelper.GetSiteLink_BibleGateway(firstVerse.Book.Name) + $"{firstVerse.Chapter}:{firstVerse.VerseId}";

                    var messageResult = title + $" <a href='{link2001}'>(2001)</a> <a href='{linkBibleHub}'>(Hub)</a> <a href='{linkGateway}'>(Gate)</a>" + Environment.NewLine;

                    foreach (var verse in verses)
                    {
                        messageResult += $"{verse.Chapter}:{verse.VerseId} {ReplaceNamesForUser(user, verse.Text)}{Environment.NewLine}";
                    }

                    return messageResult;
                }
                else
                {
                    return $"Couldn't find any verses for query = {query}";
                }
            }
        }

        private string ReplaceNamesForUser(User user, string verseText)
        {
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.DevineName))
                {
                    verseText = verseText.Replace("Jehovah", user.DevineName);
                }
                if (!string.IsNullOrWhiteSpace(user.GodsSon))
                {
                    verseText = verseText.Replace("Jesus", user.GodsSon);
                }
                if (!string.IsNullOrWhiteSpace(user.Anointed))
                {
                    verseText = verseText.Replace("Anointed One", user.Anointed, StringComparison.Ordinal);
                }

                //TODO: Fix database import, Jeremiah Span tags are included when shouldn't be
                verseText = verseText.Replace("<span", "<b");
                verseText = verseText.Replace("/span", "/b");
            }

            verseText = verseText.Replace("<br>", Environment.NewLine);
            return verseText;
        }

        private async Task<string> GetAllBooks()
        {
            using (var scope = services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
                var cmd = new GetBooksQuery();
                return await mediator.Send(cmd);
            }
        }

        private async Task<string> GetBookInformation(string bookTitle)
        {
            using (var scope = services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
                var cmd = new GetBookInfoQuery(bookTitle);

                try
                {
                    var bookInfo = await mediator.Send(cmd);
                    if (bookInfo?.Book != null)
                    {
                        string bookSiteLink = SiteHelper.GetSiteLink_2001(bookInfo.Book.Name);
                        return $@"<b>{bookInfo.Book.Name}</b> (<a href='{bookSiteLink}'>2001</a>){Environment.NewLine}Chapters: {bookInfo.Chapters}{Environment.NewLine}Verses: {bookInfo.Verses}{Environment.NewLine}{bookInfo.Book.Introduction
                            .Replace("<span", "<code").Replace("</span>", "</code>")
                            .Replace("<sup", "<code").Replace("</sup>", "</code>")}";
                    }
                    else return $"Couldn't find any title information for book {bookTitle}";
                }
                catch
                {
                    return $"Couldn't find any title information for book \"{bookTitle}\"";
                }
            }
        }

        private IEnumerable<KeyboardButton> GetSettingsReplyMarkup()
        {
            var buttons = new List<KeyboardButton>();
            buttons.Add(new KeyboardButton("Devine Name"));
            buttons.Add(new KeyboardButton("Jesus Name"));
            buttons.Add(new KeyboardButton("Anointed Name"));
            return buttons;
        }

        private IEnumerable<KeyboardButton> GetStartupReplyMarkup()
        {
            var buttons = new List<KeyboardButton>();
            buttons.Add(new KeyboardButton("/Books"));
            return buttons;
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}