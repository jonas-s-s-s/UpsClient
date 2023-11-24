using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive;
using UpsClient.Models;
using UpsClient.Views;

namespace UpsClient.ViewModels;

public partial class GameRoomViewModel : ViewModelBase
{
    private static readonly SolidColorBrush DEFAULT_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    private static readonly SolidColorBrush MY_LINE_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 255));
    private static readonly SolidColorBrush MY_CLAIMED_LINE_BRUSH = new SolidColorBrush(Color.FromRgb(10, 10, 255));
    private static readonly SolidColorBrush OPPONENT_LINE_BRUSH = new SolidColorBrush(Color.FromRgb(255, 0, 0));

    private GameClient _model;

    bool _isMyTurn;

    public ReactiveCommand<Unit, Unit> SubmitBtnCmd { get; }
    public bool IsMyTurn { get => _isMyTurn; set => this.RaiseAndSetIfChanged(ref _isMyTurn, value); }

    public string OpponentUsername { get; set; }
    public string GameStatus { get; set; }
    public string MyUsername { get; set; }

    IBrush _strokeE01 = DEFAULT_BRUSH;
    public IBrush strokeE01 { get => _strokeE01; set => this.RaiseAndSetIfChanged(ref _strokeE01, value); }

    IBrush _strokeE02 = DEFAULT_BRUSH;
    public IBrush strokeE02 { get => _strokeE02; set => this.RaiseAndSetIfChanged(ref _strokeE02, value); }

    IBrush _strokeE03 = DEFAULT_BRUSH;
    public IBrush strokeE03 { get => _strokeE03; set => this.RaiseAndSetIfChanged(ref _strokeE03, value); }

    IBrush _strokeE04 = DEFAULT_BRUSH;
    public IBrush strokeE04 { get => _strokeE04; set => this.RaiseAndSetIfChanged(ref _strokeE04, value); }

    IBrush _strokeE05 = DEFAULT_BRUSH;
    public IBrush strokeE05 { get => _strokeE05; set => this.RaiseAndSetIfChanged(ref _strokeE05, value); }

    IBrush _strokeE12 = DEFAULT_BRUSH;
    public IBrush strokeE12 { get => _strokeE12; set => this.RaiseAndSetIfChanged(ref _strokeE12, value); }

    IBrush _strokeE13 = DEFAULT_BRUSH;
    public IBrush strokeE13 { get => _strokeE13; set => this.RaiseAndSetIfChanged(ref _strokeE13, value); }

    IBrush _strokeE14 = DEFAULT_BRUSH;
    public IBrush strokeE14 { get => _strokeE14; set => this.RaiseAndSetIfChanged(ref _strokeE14, value); }

    IBrush _strokeE15 = DEFAULT_BRUSH;
    public IBrush strokeE15 { get => _strokeE15; set => this.RaiseAndSetIfChanged(ref _strokeE15, value); }

    IBrush _strokeE23 = DEFAULT_BRUSH;
    public IBrush strokeE23 { get => _strokeE23; set => this.RaiseAndSetIfChanged(ref _strokeE23, value); }

    IBrush _strokeE24 = DEFAULT_BRUSH;
    public IBrush strokeE24 { get => _strokeE24; set => this.RaiseAndSetIfChanged(ref _strokeE24, value); }

    IBrush _strokeE25 = DEFAULT_BRUSH;
    public IBrush strokeE25 { get => _strokeE25; set => this.RaiseAndSetIfChanged(ref _strokeE25, value); }

    IBrush _strokeE34 = DEFAULT_BRUSH;
    public IBrush strokeE34 { get => _strokeE34; set => this.RaiseAndSetIfChanged(ref _strokeE34, value); }

    IBrush _strokeE35 = DEFAULT_BRUSH;
    public IBrush strokeE35 { get => _strokeE35; set => this.RaiseAndSetIfChanged(ref _strokeE35, value); }

    IBrush _strokeE45 = DEFAULT_BRUSH;
    public IBrush strokeE45 { get => _strokeE45; set => this.RaiseAndSetIfChanged(ref _strokeE45, value); }

    public string? lastClickedEdge = null;

    public GameRoomViewModel(GameClient model)
    {
        _model = model;
        SubmitBtnCmd = ReactiveCommand.Create(SubmitBtn_Click);

        //Get from model
        OpponentUsername = _model.getOpponentUsername();
        MyUsername = _model.getMyUsername();
        IsMyTurn = _model.isMyTurn();

        GameStatus = "Waiting...";
        _resetAllStrokes();
    }

    public void SubmitBtn_Click()
    {
        if (lastClickedEdge == null)
            return;

        _model.claimEdge(lastClickedEdge);
        //IsOurTurn = false;

        //TODO: Await response?

        _setEdgeColor(lastClickedEdge, MY_CLAIMED_LINE_BRUSH);
        //Upadate GameStatus
        lastClickedEdge = null;
    }

    public void ClaimEdge(string edge)
    {
        IBrush? stroke = _stringToStroke(edge);
        if (stroke == null)
            throw new System.Exception("ClaimEdge() the edge " + edge + " doesn't exist.");

        //Check if it's our turn
        if (!_isMyTurn)
            return;

        //Check if this edge isn't already claimed by opponent or by us
        if (stroke == OPPONENT_LINE_BRUSH || stroke == MY_CLAIMED_LINE_BRUSH)
            return;

        //If user is trying to de-select edge
        if (stroke == MY_LINE_BRUSH)
        {
            _setEdgeColor(edge, DEFAULT_BRUSH);
            lastClickedEdge = null;
            return;
        }

        //This edge is not claimed yet
        if (stroke == DEFAULT_BRUSH)
        {
            _setEdgeColor(edge, MY_LINE_BRUSH);
            if (lastClickedEdge != null)
            {
                _setEdgeColor(lastClickedEdge, DEFAULT_BRUSH);
            }
            lastClickedEdge = edge;
        }
    }

    private void _resetAllStrokes()
    {
        strokeE01 = DEFAULT_BRUSH;
        strokeE02 = DEFAULT_BRUSH;
        strokeE03 = DEFAULT_BRUSH;
        strokeE04 = DEFAULT_BRUSH;
        strokeE05 = DEFAULT_BRUSH;

        strokeE12 = DEFAULT_BRUSH;
        strokeE13 = DEFAULT_BRUSH;
        strokeE14 = DEFAULT_BRUSH;
        strokeE15 = DEFAULT_BRUSH;

        strokeE23 = DEFAULT_BRUSH;
        strokeE24 = DEFAULT_BRUSH;
        strokeE25 = DEFAULT_BRUSH;

        strokeE34 = DEFAULT_BRUSH;
        strokeE35 = DEFAULT_BRUSH;

        strokeE45 = DEFAULT_BRUSH;
    }

    private IBrush? _stringToStroke(string edge)
    {
        switch (edge)
        {
            case "{0,1}":
                return _strokeE01;
            case "{0,2}":
                return _strokeE02;
            case "{0,3}":
                return _strokeE03;
            case "{0,4}":
                return _strokeE04;
            case "{0,5}":
                return _strokeE05;
            case "{1,2}":
                return _strokeE12;
            case "{1,3}":
                return _strokeE13;
            case "{1,4}":
                return _strokeE14;
            case "{1,5}":
                return _strokeE25;
            case "{2,3}":
                return _strokeE23;
            case "{2,4}":
                return _strokeE24;
            case "{2,5}":
                return _strokeE25;
            case "{3,4}":
                return _strokeE34;
            case "{3,5}":
                return _strokeE35;
            case "{4,5}":
                return _strokeE45;
        }
        return null;
    }

    private void _setEdgeColor(string edge, IBrush color)
    {
        switch (edge)
        {
            case "{0,1}":
                strokeE01 = color;
                return;
            case "{0,2}":
                strokeE02 = color;
                return;
            case "{0,3}":
                strokeE03 = color;
                return;
            case "{0,4}":
                strokeE04 = color;
                return;
            case "{0,5}":
                strokeE05 = color;
                return;
            case "{1,2}":
                strokeE12 = color;
                return;
            case "{1,3}":
                strokeE13 = color;
                return;
            case "{1,4}":
                strokeE14 = color;
                return;
            case "{1,5}":
                strokeE15 = color;
                return;
            case "{2,3}":
                strokeE23 = color;
                return;
            case "{2,4}":
                strokeE24 = color;
                return;
            case "{2,5}":
                strokeE25 = color;
                return;
            case "{3,4}":
                strokeE34 = color;
                return;
            case "{3,5}":
                strokeE35 = color;
                return;
            case "{4,5}":
                strokeE45 = color;
                return;
        }
    }
}
