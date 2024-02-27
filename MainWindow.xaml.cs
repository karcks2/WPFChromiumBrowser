using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFChromiumBrowser
{
    public partial class MainWindow : Window
    {
        private List<string> globalSearchHistory = new List<string>();
        private Dictionary<TabItem, List<string>> tabNavigationHistory = new Dictionary<TabItem, List<string>>();
        private HashSet<string> uniqueSearchQueries = new HashSet<string>();

        public MainWindow()
        {
            InitializeComponent();

            // Attach the SelectionChanged event handler to the TabControl
            tabControl.SelectionChanged += tabControl_SelectionChanged;

            // Populate history list when initializing the application
            PopulateHistoryList();
        }

        private void ChromiumWebBrowser_OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateNavigationButtons();
                UpdateSearchBox();

                if (sender is ChromiumWebBrowser browser && tabControl.SelectedItem is TabItem selectedTab)
                {
                    if (tabNavigationHistory.ContainsKey(selectedTab))
                    {
                        // Update navigation history for the selected tab
                        tabNavigationHistory[selectedTab].Add(browser.Address);
                    }

                    // Update the history list for the selected tab
                    UpdateTabHistoryList(selectedTab);
                }
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
            NavigateSelectedTab("https://www.google.com");
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
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            string searchQuery = boxSearch.Text.Trim();

                            // Check if the search query is not empty
                            if (!string.IsNullOrWhiteSpace(searchQuery))
                            {
                                string googleSearchUrl;

                                // Check if the search query contains a valid URL
                                if (IsUrl(searchQuery))
                                {
                                    // If it's a valid URL, navigate to it directly
                                    googleSearchUrl = searchQuery;
                                }
                                else
                                {
                                    // If it's not a valid URL, perform a Google search
                                    googleSearchUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(searchQuery);
                                }

                                // Save each navigation step (search or URL visit) in the tab's history
                                if (!tabNavigationHistory.ContainsKey(selectedTab))
                                {
                                    tabNavigationHistory[selectedTab] = new List<string>();
                                }

                                // Check if the search query is not already present in the tab's history
                                if (!tabNavigationHistory[selectedTab].Contains(googleSearchUrl))
                                {
                                    // Add the search query to the tab's history
                                    tabNavigationHistory[selectedTab].Add(googleSearchUrl);

                                    // Navigate to the search URL
                                    NavigateSelectedTab(googleSearchUrl);
                                }

                                // Check if the search query is not already present in the global search history
                                if (uniqueSearchQueries.Add(googleSearchUrl))
                                {
                                    // Add the search query to the global search history
                                    globalSearchHistory.Insert(0, googleSearchUrl);

                                    // Limit the global history to, for example, 10 items
                                    if (globalSearchHistory.Count > 10)
                                    {
                                        globalSearchHistory.RemoveAt(globalSearchHistory.Count - 1);
                                    }
                                }
                            }

                            // Clear the search bar text for the new tab
                            boxSearch.Text = "";

                            // Recreate the ChromiumWebBrowser control to refresh the entire tab content
                            var newBrowser = new ChromiumWebBrowser();
                            newBrowser.Address = selectedBrowser.Address;
                            newBrowser.FrameLoadEnd += ChromiumWebBrowser_OnFrameLoadEnd;

                            dockPanel.Children.Clear();
                            dockPanel.Children.Add(newBrowser);

                            // Update the history list for the selected tab
                            UpdateTabHistoryList(selectedTab);

                            // Add existing history items to the history list
                            PopulateHistoryList();
                        }
                    }
                }
            }
        }







        private void newTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void AddNewTab(string initialUrl = "https://www.google.com")
        {
            TabItem newTab = new TabItem();
            newTab.Header = "New Tab";

            ChromiumWebBrowser newBrowser = new ChromiumWebBrowser();
            newBrowser.Address = initialUrl; // Set the initial URL for new tabs
            newBrowser.FrameLoadEnd += ChromiumWebBrowser_OnFrameLoadEnd;

            newTab.Content = new Grid
            {
                Children = { new DockPanel { Children = { newBrowser } } }
            };

            // Initialize navigation history for the new tab
            tabNavigationHistory[newTab] = new List<string> { newBrowser.Address };

            tabControl.Items.Add(newTab);

            // Select the newly added tab
            tabControl.SelectedItem = newTab;

            // Clear the search bar text for the new tab
            boxSearch.Text = "";

            // Update the history list for the selected tab
            UpdateTabHistoryList(newTab);

            // Add existing history items to the history list
            PopulateHistoryList();
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateNavigationButtons();
            UpdateSearchBox();

            // Update history list when the selected tab changes
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                UpdateTabHistoryList(selectedTab);
            }
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
                            string baseUrl = "https://www.google.com";
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
                if (tabNavigationHistory.ContainsKey(selectedTab))
                {
                    // Update navigation history for the selected tab
                    tabNavigationHistory[selectedTab].Add(url);
                }

                if (selectedTab.Content is Grid grid)
                {
                    if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                    {
                        if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                        {
                            // Set the correct URL for the selected browser
                            selectedBrowser.Address = url;

                            // Ensure that the browser is properly loaded
                            selectedBrowser.Load(url);
                        }
                    }
                }
            }
        }

        private void NavigateSelectedTabForward()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (tabNavigationHistory.ContainsKey(selectedTab))
                {
                    List<string> history = tabNavigationHistory[selectedTab];
                    int currentIndex = history.IndexOf(GetSelectedTabUrl(selectedTab));

                    if (currentIndex < history.Count - 1)
                    {
                        string nextUrl = history[currentIndex + 1];
                        NavigateSelectedTab(nextUrl);
                    }
                }
            }
        }

        private void NavigateSelectedTabBack()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                if (tabNavigationHistory.ContainsKey(selectedTab))
                {
                    List<string> history = tabNavigationHistory[selectedTab];
                    int currentIndex = history.IndexOf(GetSelectedTabUrl(selectedTab));

                    if (currentIndex > 0)
                    {
                        string previousUrl = history[currentIndex - 1];
                        NavigateSelectedTab(previousUrl);
                    }
                }
            }
        }

        private void RefreshSelectedTab()
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                string currentUrl = GetSelectedTabUrl(selectedTab);
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    NavigateSelectedTab(currentUrl);
                }
            }
        }

        private string GetSelectedTabUrl(TabItem selectedTab)
        {
            if (selectedTab.Content is Grid grid)
            {
                if (grid.Children.Count > 0 && grid.Children[0] is DockPanel dockPanel)
                {
                    if (dockPanel.Children.Count > 0 && dockPanel.Children[0] is ChromiumWebBrowser selectedBrowser)
                    {
                        return selectedBrowser.Address;
                    }
                }
            }
            return null;
        }

        private void buttonHistory_Click(object sender, RoutedEventArgs e)
        {
            ToggleHistoryVisibility();
        }

        private void ToggleHistoryVisibility()
        {
            if (listBoxHistory.Visibility == Visibility.Collapsed)
            {
                PopulateHistoryList();
                listBoxHistory.Visibility = Visibility.Visible;
            }
            else
            {
                listBoxHistory.Visibility = Visibility.Collapsed;
            }
        }

        private void PopulateHistoryList()
        {
            listBoxHistory.Items.Clear();

            // Display the global search history including full links
            foreach (string historyItem in globalSearchHistory.OrderByDescending(item => globalSearchHistory.IndexOf(item)))
            {
                // Check if it's a valid URL before adding to history
                if (IsUrl(historyItem))
                {
                    listBoxHistory.Items.Add(historyItem);
                }
            }
        }

        private void listBoxHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxHistory.SelectedItem != null)
            {
                string selectedHistoryItem = listBoxHistory.SelectedItem.ToString();

                // Perform a search in the toolbar search menu
                boxSearch.Text = selectedHistoryItem;
                PerformSearch();

                // Hide the history list
                listBoxHistory.Visibility = Visibility.Collapsed;
                listBoxHistory.SelectedItem = null; // Clear the selection to allow future selections
            }
        }

        private void UpdateTabHistoryList(TabItem tab)
        {
            if (tabNavigationHistory.ContainsKey(tab))
            {
                // Clear and update the history list for the selected tab with unique entries
                listBoxHistory.Items.Clear();
                foreach (string historyItem in tabNavigationHistory[tab].OrderByDescending(item => tabNavigationHistory[tab].IndexOf(item)))
                {
                    // Check if it's a valid URL before adding to history
                    if (IsUrl(historyItem))
                    {
                        listBoxHistory.Items.Add(historyItem);
                    }
                }
            }
        }
        private bool IsUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out Uri resultUri)
                && (resultUri.Scheme == Uri.UriSchemeHttp || resultUri.Scheme == Uri.UriSchemeHttps)
                && resultUri.Host != null;
        }

    }
}
