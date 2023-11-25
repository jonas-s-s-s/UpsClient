using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using UpsClient.Utils;
using UpsClient.ViewModels;
using System.Threading.Tasks;
using static UpsClient.Utils.ProtocolSerializer;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

namespace UpsClient.Models;

public class GameClient
{
    private const int CUMMULATIVE_RECCONECT_WAIT = 13500; //3500
    private static readonly int PING_INTERVAL = 29000;

    private MainViewModel _mainVm;
    private ClientConnection _clientConnection;
    private Thread? _listenerThread = null;
    private volatile bool _isListening = false;

    private PeriodicTimer? _pingTimer = null;

    ConcurrentDictionary<string, string> _clientData = new ConcurrentDictionary<string, string>();

    private Action<List<RoomListItem>>? _addRoomListItemCallback = null;
    private Action<string>? _opponentUsernameCallback = null;
    private Action<string>? _myUsernameCallback = null;
    private Action<string>? _gameStatusCallback = null;
    private Action<bool>? _isMyTurnCallback = null;
    private Action<string, string>? _graphUpdateCallback = null;
    private Action<string> _connectionLostCallback = null;

    public GameClient(MainViewModel mainVm)
    {
        _mainVm = mainVm;
        _clientConnection = new ClientConnection();
    }

    private void _startListenerLoop()
    {
        if (_isListening)
            throw new Exception("_startListenerLoop(): Listening must first be stopped before calling this method.");

        _listenerThread = new Thread(_listenerLoopAsync);
        _listenerThread.IsBackground = true;
        _isListening = true;
        _listenerThread.Start();
    }
    private void _stopListenerLoop()
    {
        _isListening = false;
        if (_listenerThread != null)
        {
            _listenerThread.Interrupt();
            _listenerThread.Join();
        }
    }
    //# Listening for server messages and message processing
    //##################################################################################

    private async void _listenerLoopAsync()
    {
        while (_isListening && _clientConnection.isAlive())
        {
            try
            {
                List<ProtocolData>? data = _clientConnection.readMsg();
                if (data != null)
                {
                    //Process each received message
                    foreach (ProtocolData msg in data)
                    {
                        await _processServerMessageAsync(msg);
                    }
                }
            }
            catch (IOException ex)
            {
                //Server connection errored
                if(!await _attemptToReconnect(ex))
                {
                    return;
                }
            }
        }

        if(_isListening && !_clientConnection.isAlive() )
        {
            //Server connection closed
            if (!await _attemptToReconnect(new IOException("Connection closed.")))
            {
                return;
            }
        }
    }

    private async Task<bool> _attemptToReconnect(IOException ex)
    {
        _killGamePinger();
        _clientData["isInIdleRoom"] = "yes";
        _isMyTurnCallback?.Invoke(false);

        string host = _clientConnection.hostname;
        string port = _clientConnection.port;

        _mainVm.turnOnConnectionlostView();
        int tries = 3;
        for (int i = 1; i < tries + 1; i++)
        {
            _connectionLostCallback?.Invoke("Error message: " + ex.Message + "\nAttempting to reconnect (" + i + "/" + tries + ")");
            if (i != 1)
                System.Threading.Thread.Sleep(CUMMULATIVE_RECCONECT_WAIT * i);
            try
            {
                _clientConnection = new ClientConnection();
                await _clientConnection.connect(host, port);
            }
            catch (Exception)
            {
                _clientConnection = null;
            }

            if (_clientConnection != null)
            {
                _mainVm.turnOffConnectionlostView();
                if (_clientData.ContainsKey("myUsername"))
                    await setUsername(_clientData["myUsername"]);
                return true;
            }
        }

        _connectionLostCallback?.Invoke("Failed to reconnect. Killing connection.");
        Thread.Sleep(4000);

        _clientData.Clear();
        _clientConnection = new ClientConnection();
        _mainVm.changeToServerIpView();
        _isListening = false;

        return false;
    }

    private async void _gamePingerLoop(PeriodicTimer periodicTimer)
    {
        try
        {
            while (await periodicTimer.WaitForNextTickAsync())
            {
                Console.WriteLine("Sending ping.");
                await _clientConnection.sendMsg(newProtocolMessage(MethodName.GAME_PING));
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Sending ping failed.");
        }
    }

    private async Task _processServerMessageAsync(ProtocolData msg)
    {
        Console.WriteLine("_processServerMessage() called");

        switch (msg.Method)
        {
            case MethodName.CONNECTED_OK:
                Console.WriteLine("Connected ok.");
                _killGamePinger();

                //Return to idle room if opponent leaves game
                if (_clientData.ContainsKey("myUsername"))
                {
                    _mainVm.changeToIdleRoomView();
                    _clientData["isInIdleRoom"] = "yes";
                    await updateRoomList();
                }

                break;
            case MethodName.REQ_ACCEPTED:
                Console.WriteLine("Server request accepted.");

                //Check if msg contains room_list
                if (msg.hasField("room_list") && _addRoomListItemCallback != null)
                {
                    string room_list = msg.getField("room_list");
                    _addRoomListItemCallback(RoomListItem.ParseRoomList(room_list));
                    break;
                }

                //Try to update the room list
                await updateRoomList();

                break;
            case MethodName.REQ_DENIED:
                Console.WriteLine("Server request denied.");
                break;
            case MethodName.GAME_IDLE:
                await updateRoomList();
                break;
            case MethodName.GAME_STATE:
                if (_clientData["isInIdleRoom"] == "yes")
                {
                    _clientData["isInIdleRoom"] = "no";
                    _mainVm.changeToGameRoomView();
                    //Start pinging
                    _pingTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(PING_INTERVAL));
                    _gamePingerLoop(_pingTimer);
                }

                string gameState = msg.getField("game_state");

                if (gameState == "PAUSED")
                {
                    _gameStatusCallback?.Invoke("Game is paused.");

                    _isMyTurnCallback?.Invoke(false);
                    break;
                }

                _processGameStateUpdate(msg);
                break;
            case MethodName.GAME_PING:
                break;
        }
    }

    private void _processGameStateUpdate(ProtocolData msg)
    {
        string p1 = msg.getField("player1_username");
        string p2 = msg.getField("player2_username");
        string onTurn = msg.getField("on_turn");
        string myUsername = _clientData["myUsername"];

        _myUsernameCallback?.Invoke(myUsername);

        _opponentUsernameCallback?.Invoke((p1 != myUsername) ? p1 : p2);

        if (msg.hasField("winning_player"))
        {
            string winner = msg.getField("winning_player");
            _gameStatusCallback?.Invoke((winner == myUsername) ? "You've won." : "You've lost.");
            _isMyTurnCallback?.Invoke(false);
        }
        else
        {
            _gameStatusCallback?.Invoke((myUsername == onTurn) ? "Your turn." : "Opponent's turn.");
            _isMyTurnCallback?.Invoke(myUsername == onTurn);
        }

        string myEdges = (p1 == myUsername) ? msg.getField("player1_edges") : msg.getField("player2_edges");
        string opponentEdges = (p1 != myUsername) ? msg.getField("player1_edges") : msg.getField("player2_edges");
        _graphUpdateCallback?.Invoke(myEdges, opponentEdges);

        if (msg.hasField("winning_player"))
        {
            //Wait a bit, then reset the playing field
            System.Threading.Thread.Sleep(5000);
            _graphUpdateCallback?.Invoke("", "");
            _gameStatusCallback?.Invoke((myUsername == onTurn) ? "Your turn." : "Opponent's turn.");
            _isMyTurnCallback?.Invoke(myUsername == onTurn);
        }

    }

    //# Methods for sending messages to server 
    //##################################################################################

    public async Task connect(string host, string port)
    {
        Console.WriteLine("connect() called with: host: " + host + " port: " + port);

        try
        {
            await _clientConnection.connect(host, port);
            //After we're connected start listening
            _startListenerLoop();
            _mainVm.changeToLoginView();
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot connect to this server. Reason:\n" + ex.Message, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
        }
    }

    public async Task leaveGame()
    {
        Console.WriteLine("leaveGame() called");
        _mainVm.changeToIdleRoomView();

        _killGamePinger();

        try
        {
            await _clientConnection.sendMsg(newProtocolMessage(MethodName.GAME_LEAVE));
            _clientData["isInIdleRoom"] = "yes";
            await updateRoomList();
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to leave game. Reason:\n" + ex.Message, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
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

        _killGamePinger();

        //Terminate the listener thread
        _stopListenerLoop();

        _clientData.Clear();

        _clientConnection = new ClientConnection();
        _mainVm.changeToServerIpView();
    }

    private void _killGamePinger()
    {
        _pingTimer?.Dispose();
        _pingTimer = null;
    }

    public async Task setUsername(string username)
    {
        Console.WriteLine("setUsername() called with " + username);

        try
        {
            await _clientConnection.sendMsg(newProtocolMessage(MethodName.ENTER_USERNAME, ("username", username)));
            _clientData["myUsername"] = username;
            _clientData["isInIdleRoom"] = "yes";
            _mainVm.changeToIdleRoomView();
            await updateRoomList();
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to enter username. Reason:\n" + ex.Message, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
        }
    }

    public async Task joinGame(int id)
    {
        Console.WriteLine("joinGame() called with " + id);

        try
        {
            await _clientConnection.sendMsg(newProtocolMessage(MethodName.JOIN_GAME, ("game_id", id.ToString())));
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to enter join game.  Reason:\n" + ex.Message, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
        }
    }
    public async Task updateRoomList()
    {
        string value = "";
        if (_clientData.TryGetValue("isInIdleRoom", out value) && _addRoomListItemCallback != null)
        {
            if (value == "yes")
            {
                try
                {
                    await _clientConnection.sendMsg(newProtocolMessage(MethodName.GET_ROOM_LIST));
                }
                catch { }
            }
        }
    }

    public async Task claimEdge(string edge)
    {
        Console.WriteLine("claimEdge() called with " + edge);

        try
        {
            await _clientConnection.sendMsg(newProtocolMessage(MethodName.GAME_COMMAND, ("add_edge", edge)));
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to claim edge. Reason:\n" + ex.Message, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
        }
    }

    //# Callback setters
    //##################################################################################

    public void setAddRoomListItemCallback(Action<List<RoomListItem>> callback)
    {
        _addRoomListItemCallback = callback;
    }

    public void setOpponentUsernameCallback(Action<string> callback)
    {
        _opponentUsernameCallback = callback;
    }

    public void setMyUsernameCallback(Action<string> callback)
    {
        _myUsernameCallback = callback;
    }

    public void setGameStatusCallback(Action<string> callback)
    {
        _gameStatusCallback = callback;
    }

    public void setIsMyTurnCallback(Action<bool> callback)
    {
        _isMyTurnCallback = callback;
    }

    public void setGraphUpdateCallback(Action<string, string> callback)
    {
        _graphUpdateCallback = callback;
    }

    public void setConnectionLostCallback(Action<string> callback)
    {
        _connectionLostCallback = callback;
    }
}
