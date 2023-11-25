using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using UpsClient.Models;
using UpsClient.ViewModels;

namespace UpsClient.Views
{
    public partial class IdleRoomView : UserControl
    {
        int _lastJoinedRoom = -1;


        public IdleRoomView()
        {
            InitializeComponent();
        }

        public async void Item_PointerPressed(object sender, PointerPressedEventArgs args)
        {
            if (sender == null)
                return;

            Control control = (Control)sender;
            int roomId = (int)control.Tag;

            RoomListItem item = (RoomListItem)control.DataContext;

            //Only "Idle" games can be joined
            if (item.state != "Idle")
            {
                return;
            }

            IdleRoomViewModel viewModel = (IdleRoomViewModel)DataContext;

            if (viewModel.wasRefreshed())
            {
                _lastJoinedRoom = -1;
            }

            //Protect against repeatedly joining the same room
            if ((!String.IsNullOrEmpty(item.player1) || !String.IsNullOrEmpty(item.player2)) && _lastJoinedRoom == roomId)
            {
                return;
            }

            _lastJoinedRoom = roomId;
            await viewModel.joinGame(roomId);
        }
    }
}
