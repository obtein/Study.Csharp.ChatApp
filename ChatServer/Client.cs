using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatServer.Net.IO;

namespace ChatServer
{
     class Client
    {
        public string UserName { get; set; }
        public string UserRegion { get; set; }
        public Guid UserUID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UserUID = Guid.NewGuid();
            packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = packetReader.ReadByte();
            UserName = packetReader.ReadMessage();
            UserRegion = packetReader.ReadMessage();

            //Console.WriteLine($"[{DateTime.Now}] {UserName} has connected from {UserRegion}");

            Task.Run(() => Process());
        }


        void Process ()
        {
            while ( true )
            {
                try
                {
                    var opcode = packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = packetReader.ReadMessage();
                            Console.WriteLine($"{DateTime.Now}: Message received => {msg}");
                            Program.BroadcastMessage($"{DateTime.Now} : [{UserName}] : {msg}");
                            break;
                        default:
                            break;
                    }
                }
                catch ( Exception )
                {
                    Console.WriteLine( $"{DateTime.Now}: {UserUID} disconnected" );
                    Program.BroadcastDisconnect(UserUID.ToString());
                    ClientSocket.Close();
                }
            }
        }

    }
}
