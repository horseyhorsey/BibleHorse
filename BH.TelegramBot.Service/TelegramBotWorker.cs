using Telegram.Bot.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using BH.Application.Features.Commands;
using BH.Application.Interface;
using BH.Infrastructure.Parsers;
using MediatR;
using BH.Application.Features.Queries;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Update = Telegram.Bot.Types.Update;

namespace BH.TelegramBot.Service
{
    public class TelegramBotWorker : BackgroundService
    {
        private readonly IServiceProvider services;
        private readonly IConfiguration configuration;
        private readonly ILogger<TelegramBotWorker> _logger;

        public TelegramBotWorker(IServiceProvider services, IConfiguration configuration, ILogger<TelegramBotWorker> logger)
        {
            this.services = services;
            this.configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
            _logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");

            //process a command message
            if (message.Text.StartsWith("/"))
            {
                if (message.Text == "/start")
                {
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: Messages.MSG_WELCOME,
                    parseMode: ParseMode.Html, disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }
                else if (message.Text.StartsWith("/books"))
                {
                    var bookList = await GetAllBooks();

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: bookList,
                    parseMode: ParseMode.Html, disableWebPagePreview: true,
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
                        parseMode: ParseMode.Html, disableWebPagePreview: true,
                        cancellationToken: cancellationToken);
                    }
                }
            }
            //process standard
            else if(message.Text.Length > 5)
            {
                try
                {
                    var result = await GetVerses(message.Text);

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: result,
                    parseMode: ParseMode.Html, disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Couldn't find any verses for query = {message.Text}",
                    parseMode: ParseMode.Html, disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
                }                   
            }
        }

        private async Task<string> GetVerses(string query)
        {
            using (var scope = services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
                var cmd = new GetVersesQuery(new VersesQueryDto() { Query = query});
                var verses = await mediator.Send(cmd);

                if(verses !=null && verses.Count() > 0)
                {                    
                    var title = verses.ElementAt(0)?.Book.Name;
                    var messageResult = title + Environment.NewLine;

                    foreach (var verse in verses)
                    {
                        messageResult += $"{verse.Chapter}:{verse.VerseId} {verse.Text.Replace("<br>",Environment.NewLine)}{Environment.NewLine}";
                    }

                    return messageResult;
                }
                else
                {
                    return $"Couldn't find any verses for query = {query}";
                }                
            }
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
                        return $@"<b>{bookInfo.Book.Name}(2001)</b>{Environment.NewLine}Chapters: {bookInfo.Chapters}{Environment.NewLine}Verses: {bookInfo.Verses}{Environment.NewLine}{bookInfo.Book.Introduction
                            .Replace("<span", "<code").Replace("</span>", "</code>")
                            .Replace("<sup", "<code").Replace("</sup>", "</code>")}";
                    }
                    else return $"Couldn't find any title information for book {bookTitle}";
                }
                catch
                {
                    return $"Couldn't find any title information for book {bookTitle}";
                }
            }
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