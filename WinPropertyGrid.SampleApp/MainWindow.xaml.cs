using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Graphics;

namespace WinPropertyGrid.SampleApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = Package.Current.DisplayName;
            var display = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
            var width = 1000;
            var height = 600;
            AppWindow.MoveAndResize(new RectInt32(
                display.WorkArea.X + (display.WorkArea.Width - width) / 2,
                display.WorkArea.Y + (display.WorkArea.Height - height) / 2,
                width,
                height));

            wpg.SelectedObject = new Customer();
        }
    }
}
