using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatClient.Net.IO;

namespace ChatClient.Net
{
     class Server
    {
        TcpClient client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectedEvent;

        public Server()
        {
            client = new TcpClient();
        }

        public void ConnectToServer (string userName, string userRegion)
        {
            if ( !client.Connected )
            {
                client.Connect("127.0.0.1", 7891);
                PacketReader = new PacketReader(client.GetStream());

                if (!string.IsNullOrEmpty(userName))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode( 0 );
                    connectPacket.WriteString( userName );
                    connectPacket.WriteString( userRegion );
                    client.Client.Send( connectPacket.GetPacketBytes() );
                }
                ReadPackets();
            }
        }

        private void ReadPackets ()
        {
            Task.Run(() => 
            {
                while ( true )
                {
                    var opcode = PacketReader.ReadByte();
                    switch ( opcode )
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectedEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("brrrr");
                            break;
                    }
                }
            } );
        }

        public void SendMessageToServer ( string message )
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteString(message);
            client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
