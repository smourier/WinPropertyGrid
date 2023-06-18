using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;

namespace WinPropertyGrid.SampleApp
{
    public partial class App : Application
    {
        private Window _window = null!;

        public App()
        {
            InitializeComponent();
        }

        [MemberNotNull(nameof(_window))]
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new Window();
            _window.Activate();
        }
    }
}
