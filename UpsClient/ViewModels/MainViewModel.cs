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


    public ReactiveCommand<Unit, Unit> DisconnectBtnCmd { get; }
    public ReactiveCommand<Unit, Unit> LeaveGameBtnCmd { get; }

    private bool _isDisconnectBtnEnabled = false;
    public bool isDisconnectBtnEnabled { get => _isDisconnectBtnEnabled; set => this.RaiseAndSetIfChanged(ref _isDisconnectBtnEnabled, value); }
    private bool _isLeaveGameBtnEnabled = false;
    public bool isLeaveGameBtnEnabled { get => _isLeaveGameBtnEnabled; set => this.RaiseAndSetIfChanged(ref _isLeaveGameBtnEnabled, value); }

    public MainViewModel()
    {
        _model = new GameClient(this);

        currentView = new ServerIpView();
        currentView.DataContext = new ServerIpViewModel(_model);

        DisconnectBtnCmd = ReactiveCommand.Create(DisconnectBtn_Click);
        LeaveGameBtnCmd = ReactiveCommand.Create(LeaveGameBtn_Click);

    }

    public void changeToServerIpView()
    {
        isDisconnectBtnEnabled = false;
        isLeaveGameBtnEnabled = false;

        currentView = new ServerIpView();
        currentView.DataContext = new ServerIpViewModel(_model);
    }

    public void changeToLoginView()
    {
        isDisconnectBtnEnabled = true;
        isLeaveGameBtnEnabled = false;

        currentView = new LoginView();
        currentView.DataContext = new LoginViewModel(_model);
    }

    public void changeToIdleRoomView()
    {
        isDisconnectBtnEnabled = true;
        isLeaveGameBtnEnabled = false;

        currentView = new IdleRoomView();
        currentView.DataContext = new IdleRoomViewModel(_model);
    }

    public void changeToGameRoomView()
    {
        isDisconnectBtnEnabled = false;
        isLeaveGameBtnEnabled = true;

        currentView = new GameRoomView();
        currentView.DataContext = new GameRoomViewModel(_model);
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
