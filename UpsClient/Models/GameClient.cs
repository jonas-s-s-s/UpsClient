using System;
using UpsClient.ViewModels;

namespace UpsClient.Models
{
    public class GameClient
    {
        private MainViewModel _mainVm;

        public GameClient(MainViewModel mainVm)
        {
            _mainVm = mainVm;
        }

        public void connect(string host, string port)
        {
            Console.WriteLine("connect() called with: host: " + host + " port: " + port);

            _mainVm.changeToLoginView();
        }

        public void leaveGame()
        {
            Console.WriteLine("leaveGame() called");

            _mainVm.changeToIdleRoomView();
        }

        public void disconnect()
        {
            Console.WriteLine("disconnect() called");

            _mainVm.changeToServerIpView();
        }

        public void setUsername(string username)
        {
            Console.WriteLine("setUsername() called with " + username);

            _mainVm.changeToIdleRoomView();
        }

        public void joinGame(int id)
        {
            Console.WriteLine("joinGame() called with " + id);

            _mainVm.changeToGameRoomView();
        }
    }
}
