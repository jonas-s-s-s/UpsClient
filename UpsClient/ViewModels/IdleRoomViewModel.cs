using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using UpsClient.Models;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class IdleRoomViewModel : ViewModelBase
{
    private GameClient _model;
    public ObservableCollection<RoomListItem> ListItems { get; }

    public IdleRoomViewModel(GameClient model)
    {
        _model = model;
        ObservableCollection<RoomListItem> roomListItems = new ObservableCollection<RoomListItem>();
        roomListItems.Add(new RoomListItem(1,"AAA", "BBB", "CCC", "DDD"));
        roomListItems.Add(new RoomListItem(2,"AA", "BB", "CC", "DD"));
        ListItems = roomListItems;
    }

    public void joinGame(int id)
    {
        _model.joinGame(id);
    }


}
