using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Reactive;
using UpsClient.Models;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private GameClient _model;

    private UserControl _currentView;
    public UserControl currentView { get => _currentView; set => this.RaiseAndSetIfChanged(ref this._currentView, value); }

    // All Views created on startup
    private GameRoomView _gameRoomView;
    private IdleRoomView _idleRoomView;
    private LoginView _loginView;
    private ServerIpView _serverIpView;

    public ReactiveCommand<Unit, Unit> DisconnectBtnCmd { get; }
    public ReactiveCommand<Unit, Unit> LeaveGameBtnCmd { get; }

    private bool _isDisconnectBtnEnabled = false;
    public bool isDisconnectBtnEnabled { get => _isDisconnectBtnEnabled; set => this.RaiseAndSetIfChanged(ref _isDisconnectBtnEnabled, value); }
    private bool _isLeaveGameBtnEnabled = false;
    public bool isLeaveGameBtnEnabled { get => _isLeaveGameBtnEnabled; set => this.RaiseAndSetIfChanged(ref _isLeaveGameBtnEnabled, value); }

    public MainViewModel()
    {
        DisconnectBtnCmd = ReactiveCommand.Create(DisconnectBtn_Click);
        LeaveGameBtnCmd = ReactiveCommand.Create(LeaveGameBtn_Click);

        _model = new GameClient(this);

        _gameRoomView = new GameRoomView();
        _gameRoomView.DataContext = new GameRoomViewModel(_model);
        
        _idleRoomView = new IdleRoomView();
        _idleRoomView.DataContext = new IdleRoomViewModel(_model);

        _loginView = new LoginView();
        _loginView.DataContext = new LoginViewModel(_model);

        _serverIpView = new ServerIpView();
        _serverIpView.DataContext = new ServerIpViewModel(_model);

        _currentView = _serverIpView;
    }

    public void changeToServerIpView()
    {
        isDisconnectBtnEnabled = false;
        isLeaveGameBtnEnabled = false;

        currentView = _serverIpView;
    }

    public void changeToLoginView()
    {
        isDisconnectBtnEnabled = true;
        isLeaveGameBtnEnabled = false;

        currentView = _loginView;
    }

    public void changeToIdleRoomView()
    {
        isDisconnectBtnEnabled = true;
        isLeaveGameBtnEnabled = false;

        currentView = _idleRoomView;
    }

    public void changeToGameRoomView()
    {
        isDisconnectBtnEnabled = false;
        isLeaveGameBtnEnabled = true;

        currentView = _gameRoomView;
    }

    //Event handlers
    //##############################################################################
    public void DisconnectBtn_Click()
    {
        _model.disconnect();
    }

    public void LeaveGameBtn_Click()
    {
        _model.leaveGame();
    }



}
