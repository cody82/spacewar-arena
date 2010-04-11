using System;

using Lidgren.Network;

namespace MinimalServer
{
	class Program
	{
		static void Main(string[] args)
		{
			// create a configuration
			NetPeerConfiguration config = new NetPeerConfiguration("Minimal");
			config.Port = 14242;

			// create and start server
			NetServer server = new NetServer(config);
			server.Start();

			while (!Console.KeyAvailable)
			{
				NetIncomingMessage msg = server.WaitMessage(100);
				if (msg != null)
				{
					switch(msg.MessageType)
					{
						case NetIncomingMessageType.DebugMessage:
						case NetIncomingMessageType.ErrorMessage:
						case NetIncomingMessageType.WarningMessage:
							// print any library message
							Console.WriteLine(msg.ReadString());
							break;

						case NetIncomingMessageType.StatusChanged:
							// print changes in connection(s) status
							NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
							string reason = msg.ReadString();
							Console.WriteLine(msg.SenderConnection + " status: " + status + " (" + reason + ")");

							break;

						case NetIncomingMessageType.Data:

							// print any data sent from any connection
							Console.WriteLine(msg.SenderConnection + " data: " + msg.ReadString());
							break;
					}
				}
			}

			server.Shutdown("Server shutting down");
		}
	}
}
