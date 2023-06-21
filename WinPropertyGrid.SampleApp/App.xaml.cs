using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;

namespace WinPropertyGrid.SampleApp
{
    public partial class App : Application
    {
        public static ResourceLoader ResourceLoader { get; } = new ResourceLoader();

        private MainWindow _window = null!;

        public App()
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = null;// "fr-FR";
            InitializeComponent();
        }

        [MemberNotNull(nameof(_window))]
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Title = ResourceLoader.GetString("AppName");
            _window.Activate();
        }
    }
}
