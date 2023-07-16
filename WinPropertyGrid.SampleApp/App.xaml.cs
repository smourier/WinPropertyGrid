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
#if DEBUG
            DebugSettings.IsXamlResourceReferenceTracingEnabled = true;
            DebugSettings.IsBindingTracingEnabled = true;
            DebugSettings.IsBindingTracingEnabled = true;
#endif

            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = null;// "fr-FR";
            InitializeComponent();
        }

        [MemberNotNull(nameof(_window))]
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow
            {
                Title = ResourceLoader.GetString("AppName")
            };
            _window.Activate();
        }
    }
}
