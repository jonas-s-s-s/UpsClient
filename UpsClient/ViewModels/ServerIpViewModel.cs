using Avalonia.Controls;
using Avalonia.Interactivity;
using Config.Net;
using ReactiveUI;
using System.Reactive;
using UpsClient.Models;
using UpsClient.Utils;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class ServerIpViewModel : ViewModelBase
{
    private GameClient _model;
    public ReactiveCommand<Unit, Unit> OkBtnCmd { get; }
    public string hostnameStr { get; set; }
    public string portStr { get; set; }

    private IMySettings _settings;

    public ServerIpViewModel(GameClient model)
    {
        _model = model;
        OkBtnCmd = ReactiveCommand.Create(OkBtn_Click);
        hostnameStr = "";
        portStr = "";

        _settings = new ConfigurationBuilder<IMySettings>().UseJsonFile("./settings.json").Build();

        if (!string.IsNullOrWhiteSpace(_settings.serverHostname))
            hostnameStr = _settings.serverHostname;

        if (!string.IsNullOrWhiteSpace(_settings.serverPort))
            portStr = _settings.serverPort;
    }

    public void OkBtn_Click()
    {
        if (!string.IsNullOrWhiteSpace(hostnameStr) && !string.IsNullOrWhiteSpace(portStr))
        {
            _settings.serverHostname = hostnameStr;
            _settings.serverPort = portStr;

            _model.connect(hostnameStr, portStr);
        }
    }



}
