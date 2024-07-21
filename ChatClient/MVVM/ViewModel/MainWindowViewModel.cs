using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;

namespace ChatClient.MVVM.ViewModel
{
     class MainWindowViewModel
    {

        public ObservableCollection<UserModel> UserList { get; set; }
        public ObservableCollection<string> MessageList { get; set; }
        public string UserName { get; set; }
        public string UserRegion { get; set; }
        public string Message { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        
        private Server server;

        public MainWindowViewModel()
        {
            UserList = new ObservableCollection<UserModel>();
            MessageList = new ObservableCollection<string>();
            server = new Server();
            server.connectedEvent += UserConnected;
            server.msgReceivedEvent += MessageReceived;
            server.userDisconnectedEvent += UserDisconnected;
            ConnectToServerCommand = new RelayCommand(o => server.ConnectToServer(UserName, UserRegion), o => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(UserRegion));
            SendMessageCommand = new RelayCommand( o => server.SendMessageToServer( Message),o => !string.IsNullOrWhiteSpace( Message ) );
        }

        private void UserDisconnected ()
        {
            var uid = server.PacketReader.ReadMessage();
            var user = UserList.Where( x => x.UserUID == uid ).FirstOrDefault();
            App.Current.Dispatcher.Invoke(() => UserList.Remove(user));
        }

        private void MessageReceived ()
        {
            var msg = server.PacketReader.ReadMessage();
            App.Current.Dispatcher.Invoke(() => MessageList.Add(msg));
        }

        private void UserConnected ()
        {
            var user = new UserModel
            {
                UserName = server.PacketReader.ReadMessage(),
                UserRegion = server.PacketReader.ReadMessage(),
                UserUID = server.PacketReader.ReadMessage()
            };

            if (!UserList.Any(x => x.UserUID == user.UserUID))
            {
                App.Current.Dispatcher.Invoke(() => UserList.Add(user));
            }

        }
    }
}
