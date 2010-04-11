using System;

using Lidgren.Network;
using System.Threading;

namespace MinimalClient
{
	class Program
	{
		static void Main(string[] args)
		{
			NetClient client = new NetClient(new NetPeerConfiguration("Minimal"));
			client.Start();

			// wait one second to allow server to start up
			Thread.Sleep(1000);

			client.Connect("localhost", 14242);

			while (!Console.KeyAvailable)
			{
				NetIncomingMessage msg = client.WaitMessage(100);
				if (msg != null)
				{
					switch (msg.MessageType)
					{
						case NetIncomingMessageType.DebugMessage:
						case NetIncomingMessageType.ErrorMessage:
						case NetIncomingMessageType.WarningMessage:
							Console.WriteLine(msg.ReadString());
							break;
						case NetIncomingMessageType.StatusChanged:
							NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
							string reason = msg.ReadString();

							Console.WriteLine("Status: " + status + " (" + reason + ")");
							if (status == NetConnectionStatus.Connected)
							{
								// Whee, we're connected - send todays date
								Console.WriteLine("Sending data...");

								NetOutgoingMessage om = client.CreateMessage();
								om.Write(DateTime.Now.ToString());
								client.SendMessage(om, NetDeliveryMethod.Unreliable);

								// ... then shutdown (will automatically disconnect any connection)
								client.Shutdown("So long");
							}
							break;
					}
				}
			}

			// in case we haven't already
			client.Shutdown("Shutting down");
		}
	}
}
