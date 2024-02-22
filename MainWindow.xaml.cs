using CefSharp;
using CefSharp.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFChromiumBrowser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;

            // Attach the SelectionChanged event handler to the TabControl
            tabControl.SelectionChanged += tabControl_SelectionChanged;
        }

        private void ChromiumWebBrowser_OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateNavigationButtons();
                UpdateSearchBox();
            }));
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            NavigateSelectedTabForward();
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigateSelectedTabBack();
        }

        private void buttonGoTo_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            NavigateSelectedTab("https://search.brave.com/");
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshSelectedTab();
        }

        private void boxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            if (!string.IsNullOrWhiteSpace(boxSearch.Text))
            {
                string searchQuery = boxSearch.Text;

                // Check if the search query contains a valid URL
                if (Uri.TryCreate(searchQuery, UriKind.Absolute, out Uri resultUri)
                    && (resultUri.Scheme == Uri.UriSchemeHttp || resultUri.Scheme == Uri.UriSchemeHttps))
                {
                    // If it's a valid URL, navigate to it directly
                    NavigateSelectedTab(searchQuery);
                }
                else
                {
                    // If it's not a valid URL, perform a Google search
                    string braveSearchUrl = "https://search.brave.com/search?q=" + Uri.EscapeDataString(searchQuery);
                    NavigateSelectedTab(braveSearchUrl);
                }
            }
        }

        private void newTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void AddNewTab()
        {
            TabItem newTab = new TabItem();
            newTab.Header = "New Tab";
            newTab.Background = new SolidColorBrush(Color.FromRgb(32, 32, 39));
            newTab.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 32, 39));
            newTab.Foreground = Brushes.White;

            ChromiumWebBrowser newBrowser = new ChromiumWebBrowser();
            newBrowser.Address = "https://search.brave.com/"; // Set the default URL for new tabs
            newBrowser.FrameLoadEnd += ChromiumWebBrowser_OnFrameLoadEnd;

            newTab.Content = new Grid
            {
                Children = { new DockPanel { Children = { newBrowser } } }
            };

            tabControl.Items.Add(newTab);

            // Select the newly added tab
            tabControl.SelectedItem = newTab;

            // Clear the search bar text for the new tab
            boxSearch.Text = "";
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateNavigationButtons();
            UpdateSearchBox();
        }

        private void UpdateNavigationButtons()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            Dispatcher.BeginInvoke((Action)(() =>
                            {
                                buttonForward.IsEnabled = selectedBrowser.CanGoForward;
                                buttonBack.IsEnabled = selectedBrowser.CanGoBack;
                            }));
                        }
                    }
                }
            }
        }

        private void UpdateSearchBox()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            // Display only the base URL in the search bar for the selected tab
                            string baseUrl = "https://search.brave.com/";
                            string fullLink = selectedBrowser.Address;

                            if (fullLink.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    boxSearch.Text = fullLink.Substring(baseUrl.Length).TrimStart('/');
                                }));
                            }
                            else
                            {
                                Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    boxSearch.Text = fullLink;
                                }));
                            }
                        }
                    }
                }
            }
        }

        private void NavigateSelectedTab(string url)
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            Dispatcher.BeginInvoke((Action)(() =>
                            {
                                selectedBrowser.Address = url;
                            }));
                        }
                    }
                }
            }
        }

        private void NavigateSelectedTabForward()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            if (selectedBrowser.CanGoForward)
                            {
                                Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    selectedBrowser.Forward();
                                }));
                            }
                        }
                    }
                }
            }
        }

        private void NavigateSelectedTabBack()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            if (selectedBrowser.CanGoBack)
                            {
                                Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    selectedBrowser.Back();
                                }));
                            }
                        }
                    }
                }
            }
        }

        private void RefreshSelectedTab()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            Dispatcher.BeginInvoke((Action)(() =>
                            {
                                selectedBrowser.Reload();
                            }));
                        }
                    }
                }
            }
        }
    }
}
