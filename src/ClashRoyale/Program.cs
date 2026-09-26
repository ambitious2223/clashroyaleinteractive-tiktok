using System;
using System.Threading;
using ClashRoyale.Core;
using ClashRoyale.Extensions.Utils;
using ClashRoyale.Utilities.Utils;

namespace ClashRoyale
{
    public static class Program
    {
        private static void Main()
        {
            Console.Title = "ZrdRoyale";
            Console.ForegroundColor = ConsoleColor.Green;
            
            Console.WriteLine(
                " _____            ______                    __   \n/__  /  _________/ / __ \\____  __  ______ _/ /__  \n  / /  / ___/ __  / /_/ / __ \\/ / / / __ `/ / _ \\ \n / /__/ /  / /_/ / _, _/ /_/ / /_/ / /_/ / /  __/ \n/____/_/   \\__,_/_/ |_|\\____/\\__, /\\__,_/_/\\___/ \n                            /____/               \n");
            Resources.Initialize();
            Console.WriteLine("Thanks to Incredible for work on orginal version of CR server");
            Console.WriteLine("Fork of RetroRoyale by Zordon1337");
           
            Console.WriteLine(Resources.Configuration.goldreward);
            Console.WriteLine(Resources.Configuration.gemsreward);

            var httpPort = 8080;
            Resources.SpawnHttp = new Core.Network.SpawnHttpServer();
            Resources.SpawnHttp.Start(httpPort);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[SpawnHTTP] Troop spawn API running on http://127.0.0.1:{httpPort}/");
            Console.WriteLine($"[SpawnHTTP] POST /spawn  |  GET /battles  |  GET /cards");
            Console.ForegroundColor = ConsoleColor.Green;

            WebhookUtils.SendNotify(Resources.Configuration.Srv_Webhook, Resources.LangConfiguration.SrvStarting, "Server Log");
            
            Logger.Log("Server is running. Press Ctrl+C or close window to stop.", null);
            
            var quitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, e) => { e.Cancel = true; quitEvent.Set(); };
            quitEvent.WaitOne();
            Shutdown();
            WebhookUtils.SendError(Resources.Configuration.Srv_Webhook, Resources.LangConfiguration.SrvClosing, "Server Log");
        }

        public static async void Shutdown()
        {
            
            Console.WriteLine("Shutting down...");

            await Resources.Netty.Shutdown();

            try
            {
                Console.WriteLine("Saving players...");

                lock (Resources.Players.SyncObject)
                {
                    foreach (var player in Resources.Players.Values) player.Save();
                }

                Console.WriteLine("All players saved.");
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't save all players.");
            }

            await Resources.Netty.ShutdownWorkers();
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}
