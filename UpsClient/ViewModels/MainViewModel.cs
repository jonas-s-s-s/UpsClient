using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
    public GameRoomView GameRoomView;
    public IdleRoomView IdleRoomView;
    public LoginView LoginView;
    public ServerIpView ServerIpView;

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

        GameRoomView = new GameRoomView();
        GameRoomView.DataContext = new GameRoomViewModel(_model);
        
        IdleRoomView = new IdleRoomView();
        IdleRoomView.DataContext = new IdleRoomViewModel(_model);

        LoginView = new LoginView();
        LoginView.DataContext = new LoginViewModel(_model);

        ServerIpView = new ServerIpView();
        ServerIpView.DataContext = new ServerIpViewModel(_model);

        _currentView = ServerIpView;
    }

    public void changeToServerIpView()
    {
        isDisconnectBtnEnabled = false;
        isLeaveGameBtnEnabled = false;

        currentView = ServerIpView;
    }

    public void changeToLoginView()
    {
        isDisconnectBtnEnabled = true;
        isLeaveGameBtnEnabled = false;

        currentView = LoginView;
    }

    public void changeToIdleRoomView()
    {
        isDisconnectBtnEnabled = true;
        isLeaveGameBtnEnabled = false;

        Dispatcher.UIThread.Invoke(() =>
        {
            IdleRoomViewModel vm = (IdleRoomViewModel)IdleRoomView.DataContext;
            vm.refresh();
        });
        currentView = IdleRoomView;
    }

    public void changeToGameRoomView()
    {
        isDisconnectBtnEnabled = false;
        isLeaveGameBtnEnabled = true;

        currentView = GameRoomView;
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
