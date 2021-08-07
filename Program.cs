using System.Windows.Forms;
using DiscordRPC;
using System;

namespace PacketClientInjector
{
	internal static class Program

	{
		[STAThread]
		private static void Main()
		{
            DiscordRpcClient client = new DiscordRpcClient("866924923338620978");
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                State = "",
                Timestamps = Timestamps.Now,
                Assets = new Assets
                {
                    LargeImageText = "Packet Client",
                    SmallImageText = "Minecraft: Bedrock Edition",
                    LargeImageKey = "icon",
                    SmallImageKey = "mcbe"
                }
            });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Application.Run(new MainForm());
        }
    }
}