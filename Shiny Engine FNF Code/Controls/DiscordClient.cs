using DiscordRPC;
using DiscordRPC.Logging;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Shiny_Engine_FNF.Code.Controls
{
    static class DiscordClient
    {
        public static DiscordRpcClient DiscordRpc;

        private static Thread _discordthread;
        private static bool _initalized;

        public static void Initalize()
        {
            if (_discordthread == null)
            {
                _discordthread = new Thread(() =>
                {
                    DiscordRpc = new DiscordRpcClient("557069829501091850");
                    DiscordRpc.OnReady += (sender, args) =>
                    {
                        DiscordRpc.SetPresence(new RichPresence()
                        {
                            Details = "In the Menus",
                            Assets = new Assets()
                            {
                                LargeImageKey = "icon",
                                LargeImageText = "fridaynightfunkin"
                            }
                        });
                    };
                    DiscordRpc.OnError += (sender, args) => Debug.WriteLine($"Error! {args.Code} : {args.Message}");
                    DiscordRpc.OnClose += (sender, args) => Debug.WriteLine($"Disconnected! {args.Code} : {args.Reason}");

                    _initalized = DiscordRpc.Initialize();
                    if (_initalized)
                    {

                        Debug.WriteLine("Discord Client started.");

                        while (!DiscordRpc.IsDisposed)
                        {
                            DiscordRpc.Invoke();
                            Thread.Sleep(150);
                            //trace("Discord Client Update");
                        }
                    }

                })
                { Name = "Discord Helper" };
                _discordthread.Start();
                Trace.WriteLine("Discord Client initialized");
            }
        }

        public static void Shutdown()
        {
            DiscordRpc?.Dispose();
            _discordthread = null;
        }

        public static void ChangePresence(string details, string state, string smallImageKey = null, bool hasStartTimestamp = false, DateTime endTimestamp = default)
        {
            DateTime? startTimestamp = hasStartTimestamp ? DateTime.Now : null;

            if (endTimestamp != default)
                endTimestamp = startTimestamp.GetValueOrDefault() + TimeSpan.FromTicks(endTimestamp.Ticks);
            if (_initalized)
            {
                DiscordRpc.ClearPresence();
                DiscordRpc.SetPresence(new RichPresence()
                {
                    Details = details,
                    State = state,
                    Timestamps = new Timestamps()
                    {
                        Start = startTimestamp,
                        End = endTimestamp
                    },
                    Assets = new Assets()
                    {
                        LargeImageKey = "icon",
                        LargeImageText = "fridaynightfunkin",
                        SmallImageKey = smallImageKey
                    }
                });

            }

            //trace('Discord RPC Updated. Arguments: $details, $state, $smallImageKey, $hasStartTimestamp, $endTimestamp');
        }
    }
}
