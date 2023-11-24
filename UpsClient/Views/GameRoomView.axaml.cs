using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using UpsClient.ViewModels;

namespace UpsClient.Views
{
    public partial class GameRoomView : UserControl
    {
        public GameRoomView()
        {
            InitializeComponent();
        }

        private void Line_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Shape edge = (Shape) sender;
            string edgeDescription = edge.Tag.ToString();

            GameRoomViewModel vm = (GameRoomViewModel) DataContext;
            vm.ClaimEdge(edgeDescription);
        }

    }
}
