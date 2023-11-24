using System;
using UpsClient.ViewModels;

namespace UpsClient.Models
{
    public class GameClient
    {
        private MainViewModel _mainVm;
        private string _myUsername = "Me";
        private string _opponentUsername = "Opponent";
        private string _gameStatus = "Waiting...";
        private bool _isMyTurn = false;

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
            _myUsername = username;

            _mainVm.changeToIdleRoomView();
        }

        public string getMyUsername()
        {
            return _myUsername;
        }

        public string getOpponentUsername()
        {
            return _opponentUsername;
        }

        public string getGameStatus()
        {
            return _gameStatus;
        }

        public bool isMyTurn()
        {
            return _isMyTurn;
        }

        public void joinGame(int id)
        {
            Console.WriteLine("joinGame() called with " + id);

            _mainVm.changeToGameRoomView();
        }

        public void claimEdge(string edge)
        {
            Console.WriteLine("claimEdge() called with " + edge);
            
            //TODO
        }
    }
}
