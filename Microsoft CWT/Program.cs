using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MicrosoftCWT
{
    public class Program
    {
        private static TelegramBotClient _botClient = null!;
        private static readonly long AllowedChatId = 11111; // ваш chat_id
        private static readonly string Token = ""; //ваш токен
        private static readonly string OnPC = $"🐉 ПК был включен! Бот запущен и готов к работе {DateTime.Now}";

        public static async Task Main()
        {
            _botClient = new TelegramBotClient(Token);

            await _botClient.SendMessage(
                chatId: AllowedChatId,
                text: OnPC,
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: CancellationToken.None);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message }
            };

            var updateHandler = new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync);

            _botClient.StartReceiving(
                updateHandler: updateHandler,
                receiverOptions: receiverOptions
            );

            await Task.Delay(-1);
        }

        private static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "🖥️ Выключить", "🔄 Перезагрузить" },
                new KeyboardButton[] { "🔒 Заблокировать", "❓ Помощь" }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            if (update.Message is not { Text: string messageText, Chat: { Id: long chatId } })
                return;

            if (chatId != AllowedChatId)
            {
                await bot.SendMessage(
                    chatId: chatId,
                    text: "⛔ Доступ запрещён!",
                    cancellationToken: ct);
                return;
            }

            switch (messageText.ToLower())
            {
                case "/shutdown":
                case "🖥️ выключить":
                case "выключить":
                    await ExecuteCommand(bot, chatId, "🖥️ Выключаю компьютер...", "shutdown", "/s /t 1", ct);
                    break;

                case "/reboot":
                case "🔄 перезагрузить":
                case "перезагрузить":
                    await ExecuteCommand(bot, chatId, "🔄 Перезагружаю компьютер...", "shutdown", "/r /t 1", ct);
                    break;

                case "/lock":
                case "🔒 заблокировать":
                case "заблокировать":
                    await ExecuteCommand(bot, chatId, "🔒 Блокирую компьютер...", "rundll32.exe", "user32.dll,LockWorkStation", ct);
                    break;

                case "/help":
                case "❓ помощь":
                case "помощь":
                    await SendHelpMessage(bot, chatId, ct);
                    break;

                default:
                    await SendHelpMessage(bot, chatId, ct);
                    break;
            }
        }

        private static async Task SendHelpMessage(ITelegramBotClient bot, long chatId, CancellationToken ct)
        {
            await bot.SendMessage(
                chatId: chatId,
                text: "Доступные команды:\n\n" +
                      "🖥️ Выключить - выключение компьютера\n" +
                      "🔄 Перезагрузить - перезагрузка компьютера\n" +
                      "🔒 Заблокировать - блокировка компьютера\n\n" +
                      "Можно использовать как кнопки, так и команды:\n" +
                      "/shutdown, /reboot, /lock",
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: ct);
        }

        private static async Task ExecuteCommand(
            ITelegramBotClient bot,
            long chatId,
            string responseText,
            string fileName,
            string arguments,
            CancellationToken ct)
        {
            await bot.SendMessage(
                chatId: chatId,
                text: responseText,
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: ct);

            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        private static Task HandleErrorAsync(ITelegramBotClient bot, Exception error, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {error.Message}");
            return Task.CompletedTask;
        }
    }
}