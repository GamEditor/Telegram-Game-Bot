using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types;

namespace TelegramBotService
{
    public partial class Service : ServiceBase
    {
        private long m_ReportGroupChatId = -278455237;  // Exception Reporter

        private TelegramBotClient m_TelegramBotClient = new TelegramBotClient("replace_your_bot_token");

        private ReplyKeyboardMarkup m_ReplyMarkupKeyboard = new ReplyKeyboardMarkup();

        private InlineKeyboardMarkup m_GameKeyboardMarkup = new InlineKeyboardMarkup();
        private InlineKeyboardMarkup m_AboutKeyboardMarkup = new InlineKeyboardMarkup();
        
        public Service()
        {
            InitializeComponent();
        }

#if DEBUG
        public void OnDebug()
        {
            OnStart(null);
        }
#endif

        protected override void OnStart(string[] args)
        {
            Init();

            // OnMessages:
            m_TelegramBotClient.OnMessage += M_TelegramBotClient_OnMessage;
            m_TelegramBotClient.OnMessageEdited += M_TelegramBotClient_OnMessage;

            // OnCallbackQueries:
            m_TelegramBotClient.OnCallbackQuery += M_TelegramBotClient_OnCallbackQuery; // inside the bot
            m_TelegramBotClient.OnInlineQuery += M_TelegramBotClient_OnInlineQuery;     // outside the bot (like forward game to users or groups, etc)

            m_TelegramBotClient.StartReceiving();
        }

        protected override void OnStop()
        {
            m_TelegramBotClient.StopReceiving();
        }

        /// <summary>
        /// Initializes arrays (keyboards are unique always)
        /// </summary>
        private void Init()
        {
            m_ReplyMarkupKeyboard.ResizeKeyboard = true;
            m_ReplyMarkupKeyboard.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Start 2048")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("About Us")
                }
            };

            m_GameKeyboardMarkup.InlineKeyboard = new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallBackGame("Start Game", new CallbackGame())
                }
            };

            m_AboutKeyboardMarkup.InlineKeyboard = new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithUrl("Telegram Channel", "https://t.me/your_channel"),
                    InlineKeyboardButton.WithUrl("Instagram", "https://www.instagram.com/your_instagram/"),
                    InlineKeyboardButton.WithUrl("About Us", "http://your_domain_name.x/x/")
                }
            };
        }

        private async void M_TelegramBotClient_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            try
            {
                if(e.CallbackQuery.IsGameQuery && e.CallbackQuery.GameShortName == "trix2048")
                    await m_TelegramBotClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, null, false, "http://79.127.47.210:76/test/index.html");
            }
            catch (Exception ex)
            {
                // respond to user:
                await m_TelegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "خطایی رخ داده! گزارش خطا به توسعه دهنده ربات ارسال خواهد شد. لطفا دوباره تلاش کنید.");

                // report to group:
                await m_TelegramBotClient.SendTextMessageAsync(m_ReportGroupChatId, ex.StackTrace);
            }
        }

        private async void M_TelegramBotClient_OnInlineQuery(object sender, Telegram.Bot.Args.InlineQueryEventArgs e)
        {
            try
            {
                await m_TelegramBotClient.AnswerCallbackQueryAsync(e.InlineQuery.Id, null, false, "http://79.127.47.210:76/test/index.html");
            }
            catch (Exception ex)
            {
                // respond to user:
                await m_TelegramBotClient.SendTextMessageAsync(e.InlineQuery.From.Id, "خطایی رخ داده! گزارش خطا به توسعه دهنده ربات ارسال خواهد شد. لطفا دوباره تلاش کنید.");

                // report to group:
                await m_TelegramBotClient.SendTextMessageAsync(m_ReportGroupChatId, ex.StackTrace);
            }
        }

        private async void M_TelegramBotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.TextMessage)
                switch (e.Message.Text)
                {
                    case "/start":
                    case "/menu":
                        await m_TelegramBotClient.SendPhotoAsync(e.Message.Chat.Id, new FileToSend(new Uri("http://s9.picofile.com/file/8320338576/bot_help.png")), "دسترسی به فهرست اصلی", false, 0, m_ReplyMarkupKeyboard);
                        break;

                    case "شروع بازی 2048":
                        await m_TelegramBotClient.SendGameAsync(e.Message.Chat.Id, "trix2048", false, 0, m_GameKeyboardMarkup);
                        break;

                    case "درباره ما":
                        await m_TelegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, "استودیو بازی سازی تریکس در رسانه ها", ParseMode.Default, false, false, 0, m_AboutKeyboardMarkup);
                        break;

                    default:
                        await m_TelegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, "این دستور مورد تایید نیست.");
                        break;
                }
        }
    }
}
