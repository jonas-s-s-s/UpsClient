using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using UpsClient.Models;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private GameClient _model;
    public ReactiveCommand<Unit, Unit> OkBtnCmd { get; }
    public string usernameStr { get; set; }

    public LoginViewModel(GameClient model)
    {
        _model = model;
        OkBtnCmd = ReactiveCommand.CreateFromTask(OkBtn_Click);
        usernameStr = string.Empty;
    }

    public async Task OkBtn_Click()
    {
        if (!string.IsNullOrWhiteSpace(usernameStr))
            await _model.setUsername(usernameStr);
    }



}
