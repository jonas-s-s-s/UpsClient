using Avalonia.Controls;
using Avalonia.Interactivity;
using Config.Net;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using UpsClient.Models;
using UpsClient.Utils;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class ConnectionLostViewModel : ViewModelBase
{
    private GameClient _model;

    private string _status = "";
    public string Status { get => _status; set => this.RaiseAndSetIfChanged(ref _status, value); }


    public ConnectionLostViewModel(GameClient model)
    {
        _model = model;
        _model.setConnectionLostCallback((Str) => {
            Status = Str;
        });
    }




}
