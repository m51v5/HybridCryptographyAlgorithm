using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HybridCryptographyAlgorithm
{
    public partial class AboutWindow : Window
    {
        readonly MainWindow main;
        UpdateResult? update;

        internal AboutWindow(MainWindow owner, UpdateResult? lastCheck)
        {
            InitializeComponent();
            Owner = owner;
            main = owner;

            versionText.Text = $"Version {AppLinks.Version}";
            SetLink(rowReleases, AppLinks.Releases);
            SetLink(rowSource, AppLinks.Repository);
            SetLink(rowSecurity, AppLinks.Security);
            SetLink(rowLicense, AppLinks.License);
            SetLink(rowReport, AppLinks.ReportProblem);
            SetLink(rowWebsite, AppLinks.Website);
            SetLink(rowTelegram, AppLinks.Telegram);
            SetLink(rowContact, AppLinks.Contact);

            if (lastCheck != null)
            {
                ShowUpdate(lastCheck);
            }
        }

        // The row opens url and shows it on hover.
        static void SetLink(Button row, string url)
        {
            row.Tag = url;
            row.ToolTip = url;
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            AppLinks.Open((string)((Button)sender).Tag);
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (update?.Status == UpdateStatus.Available && update.ReleaseUrl != null)
            {
                AppLinks.Open(update.ReleaseUrl);
                return;
            }

            rowUpdate.IsEnabled = false;
            updateText.Text = "Checking…";
            UpdateResult result = await UpdateChecker.CheckAsync();
            rowUpdate.IsEnabled = true;
            ShowUpdate(result);
            main.ShowUpdateResult(result);
        }

        void ShowUpdate(UpdateResult result)
        {
            update = result;
            updateText.Text = result.Status switch
            {
                UpdateStatus.Available => $"{result.LatestTag} is out",
                UpdateStatus.UpToDate => "Up to date",
                _ => "Couldn't reach GitHub",
            };
            rowUpdate.ToolTip = result.Status == UpdateStatus.Available
                ? $"Opens {result.ReleaseUrl}"
                : "Asks GitHub for the latest release";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Drag_Window(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
