using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using ChatServer.Net.IO;
using Microsoft.VisualBasic;

namespace ChatServer
{
    class Program
    {
        static List<Client> userList;
        static TcpListener listener;
        static void Main ( string [] args)
        {
            userList = new List<Client>();
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            listener.Start();
                while ( true )
                {
                    var client = new Client( listener.AcceptTcpClient() );
                    userList.Add( client );

                    // Broadcast the connection
                    BroadcastConnection();
                }
        } 


            static void BroadcastConnection ()
            {
                foreach ( var user in userList )
                {
                    foreach ( var usr in userList )
                    {
                        var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(1);
                        broadcastPacket.WriteString(usr.UserName);
                        broadcastPacket.WriteString( usr.UserRegion );
                        broadcastPacket.WriteString(usr.UserUID.ToString());
                        user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                    }
                }
            }

        public static void BroadcastMessage (string message)
        {
            foreach (var user in userList)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode( 5 );
                msgPacket.WriteString(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }
        public static void BroadcastDisconnect ( string uid )
        {
            var disconnectedUser = userList.Where( x => x.UserUID.ToString() == uid ).FirstOrDefault();
            userList.Remove(disconnectedUser);
            foreach ( var user in userList )
            {
                var disconnectPacket = new PacketBuilder();
                disconnectPacket.WriteOpCode(10);
                disconnectPacket.WriteString(uid);
                user.ClientSocket.Client.Send(disconnectPacket.GetPacketBytes());
            }

            BroadcastMessage($"{disconnectedUser.UserName} has disconnected");
        }


    }
    }
