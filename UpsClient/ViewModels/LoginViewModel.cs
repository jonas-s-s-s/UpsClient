using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Reactive;
using UpsClient.Models;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class IdleRoomViewModel : ViewModelBase
{
    private GameClient _model;

    public IdleRoomViewModel(GameClient model)
    {
        _model = model;
    }


}
