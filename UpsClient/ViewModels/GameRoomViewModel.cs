using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Reactive;
using UpsClient.Models;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class GameRoomViewModel : ViewModelBase
{
    private GameClient _model;
    public ReactiveCommand<Unit, Unit> OkBtnCmd { get; }
    public string usernameStr { get; set; }

    public GameRoomViewModel(GameClient model)
    {
        _model = model;
        OkBtnCmd = ReactiveCommand.Create(OkBtn_Click);
        usernameStr = string.Empty;
    }

    public void OkBtn_Click()
    {
        if (!string.IsNullOrWhiteSpace(usernameStr))
            _model.setUsername(usernameStr);
    }



}
