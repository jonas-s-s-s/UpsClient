using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using UpsClient.ViewModels;

namespace UpsClient.Views
{
    public partial class IdleRoomView : UserControl
    {
        public IdleRoomView()
        {
            InitializeComponent();
        }

        public void Item_PointerPressed(object sender, PointerPressedEventArgs args)
        {
            if (sender == null)
                return;

            Control control = (Control)sender;
            int roomId = (int)control.Tag;

            IdleRoomViewModel viewModel = (IdleRoomViewModel)DataContext;
            viewModel.joinGame(roomId);
        }
    }
}
