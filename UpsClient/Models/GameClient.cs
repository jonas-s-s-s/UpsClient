using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using UpsClient.Utils;
using UpsClient.ViewModels;
using UpsClient.Views;
using System.Threading.Tasks;
using static UpsClient.Utils.ProtocolSerializer;

namespace UpsClient.Models
{
    public class GameClient
    {
        private MainViewModel _mainVm;
        private ClientConnection _clientConnection;

        private string _myUsername = "Me";
        private string _opponentUsername = "Opponent";
        private string _gameStatus = "Waiting...";
        private bool _isMyTurn = false;

        public GameClient(MainViewModel mainVm)
        {
            _mainVm = mainVm;
            _clientConnection = new ClientConnection();
        }

        public async Task connect(string host, string port)
        {
            Console.WriteLine("connect() called with: host: " + host + " port: " + port);

            try
            {
                await _clientConnection.connect(host, port);

                System.Collections.Generic.List<ProtocolData>? protocolData = await _clientConnection.readMsg();
                Console.WriteLine();

                _mainVm.changeToLoginView();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot connect to this server. " + ex.Message, ButtonEnum.Ok, Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        public async Task leaveGame()
        {
            Console.WriteLine("leaveGame() called");

            try
            {
                await _clientConnection.sendMsg(newProtocolMessage(MethodName.GAME_LEAVE));
                _mainVm.changeToIdleRoomView();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to leave game. " + ex.Message, ButtonEnum.Ok, Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        public async Task disconnect()
        {
            Console.WriteLine("disconnect() called");

            try
            {
                await _clientConnection.sendMsg(newProtocolMessage(MethodName.TERMINATE_CONNECTION));
            }
            catch { }
            _clientConnection = new ClientConnection();
            _mainVm.changeToServerIpView();
        }

        public async Task setUsername(string username)
        {
            Console.WriteLine("setUsername() called with " + username);
            _myUsername = username;

            try
            {
                await _clientConnection.sendMsg(newProtocolMessage(MethodName.ENTER_USERNAME, ("username", username)));
                _mainVm.changeToIdleRoomView();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to enter username. " + ex.Message, ButtonEnum.Ok, Icon.Error);
                var result = await box.ShowAsync();
            }

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
