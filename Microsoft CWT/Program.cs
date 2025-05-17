using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MicrosoftCWT;

public class Program
{
    private static TelegramBotClient _botClient = null!;
    private static readonly long AllowedChatId = 12345; // ваш chat_id
    private static readonly string Token = ""; //ваш токен
    private static readonly string OnPC = $"🐉 ПК был включен! Бот запущен и готов к работе {DateTime.Now.ToString()}";

    public static async Task Main()
    {
        _botClient = new TelegramBotClient(Token);

        await _botClient.SendMessage(
                chatId: AllowedChatId,
                text: OnPC,
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
                await ExecuteCommand(bot, chatId, "🖥️ Выключаю компьютер...", "shutdown", "/s /t 1", ct);
                break;

            case "/reboot":
                await ExecuteCommand(bot, chatId, "🔄 Перезагружаю компьютер...", "shutdown", "/r /t 1", ct);
                break;

            case "/lock":
                await ExecuteCommand(bot, chatId, "🔒 Блокирую компьютер...", "rundll32.exe", "user32.dll,LockWorkStation", ct);
                break;

            default:
                await bot.SendMessage(
                    chatId: chatId,
                    text: "Доступные команды:\n/shutdown\n/reboot\n/lock",
                    cancellationToken: ct);
                break;
        }
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
// Команда сборки для Relase
// dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
