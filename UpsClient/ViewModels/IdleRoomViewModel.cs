using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using UpsClient.Models;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class IdleRoomViewModel : ViewModelBase
{
    private GameClient _model;
    public ObservableCollection<RoomListItem> ListItems { get; }
    public ReactiveCommand<Unit, Unit> RefreshBtnCmd { get; }

    private bool _refreshed = false;

    public IdleRoomViewModel(GameClient model)
    {
        ListItems = new ObservableCollection<RoomListItem>();

        _model = model;
        _model.setAddRoomListItemCallback((items) => {
            Dispatcher.UIThread.Invoke(() =>
            {
                ListItems.Clear();
                ListItems.AddRange(items);
            });
        });

        RefreshBtnCmd = ReactiveCommand.CreateFromTask(_model.updateRoomList);
    }

    public async Task joinGame(int id)
    {
        await _model.joinGame(id);
    }

    public bool wasRefreshed()
    {
        bool oldR = _refreshed;
        if(oldR)
        {
            _refreshed = false;
        }
        return oldR;
    }

    public async void refresh()
    {
        _refreshed = true;
    }
}
