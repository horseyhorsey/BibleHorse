using Telegram.Bot.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using BH.Application.Interface;
using MediatR;
using Update = Telegram.Bot.Types.Update;
using BH.Application.Features.Commands;
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

            await ProcessTelegramMessage(botClient, message, chatId, userId, cancellationToken);
        }

        private async Task ProcessTelegramMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Message message, long chatId, long userId, CancellationToken cancellationToken)
        {
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
                else if (message.Text.StartsWith("/books"))
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
                    var bookInfo = string.IsNullOrWhiteSpace(cmd) ? "Provide a book title or short name for info like /info gen" : await GetBookInformation(cmd);
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
                else if (message.Text.StartsWith("/find"))
                {
                    var searchTerm = message.Text.Replace("/find", "").Trim();                    

                    var searchResults = string.IsNullOrWhiteSpace(searchTerm) ?
                        Messages.MSG_FIND_HELP :
                        await SearchVerses(userId, searchTerm.Trim());
                    if (searchResults != null)
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: searchResults,
                        disableNotification: true,
                        parseMode: ParseMode.Html, disableWebPagePreview: true,
                        cancellationToken: cancellationToken);
                    }
                }
                else if (message.Text.StartsWith("/f"))
                {
                    var searchTerm = message.Text.Replace("/f", "").Trim();

                    var searchResults = string.IsNullOrWhiteSpace(searchTerm) ?
                        Messages.MSG_FIND_HELP :
                        await SearchVerses(userId, searchTerm.Trim());
                    if (searchResults != null)
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: searchResults,
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
            //process standard queries
            else if (message.Text.Length > 5)
            {
                var result = await GetVerses(userId, message.Text);
                await botClient.SendTextMessageAsync(
                chatId: chatId,
                disableNotification: true,
                text: result,
                parseMode: ParseMode.Html, disableWebPagePreview: true,
                cancellationToken: cancellationToken);
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

        #region Bible Service Methods
        private async Task<string> GetAllBooks()
        {
            using (var scope = services.CreateScope())
            {
                var bibleService = scope.ServiceProvider.GetRequiredService<IBibleService>();
                return await bibleService.GetAllBooks();
            }
        }

        private async Task<string> GetBookInformation(string bookTitle)
        {
            using (var scope = services.CreateScope())
            {
                var bibleService = scope.ServiceProvider.GetRequiredService<IBibleService>();
                return await bibleService.GetBookInformation(bookTitle);
            }
        }

        private async Task<string> GetVerses(long? userId, string query)
        {
            using (var scope = services.CreateScope())
            {
                var bibleService = scope.ServiceProvider.GetRequiredService<IBibleService>();
                return await bibleService.GetVerses(userId, query);
            }
        }

        private async Task<string> SearchVerses(long? userId, string query)
        {
            using (var scope = services.CreateScope())
            {
                var bibleService = scope.ServiceProvider.GetRequiredService<IBibleService>();
                return await bibleService.SearchVerses(userId, query);
            }
        }

        #endregion        

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
    }
}