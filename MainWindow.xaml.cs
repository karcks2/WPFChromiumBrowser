using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFChromiumBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ChromiumWebBrowser chrome;

        public MainWindow()
        {
            InitializeComponent();
            AddTab("https://www.google.com");
        }

        private void ChromiumWebBrowser_OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                buttonForward.IsEnabled = Browser_Test.CanGoForward;
                buttonBack.IsEnabled = Browser_Test.CanGoBack;
            }));
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            if (Browser_Test.CanGoForward)
            {
                Browser_Test.Forward();
            }
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            if (Browser_Test.CanGoBack)
            {
                Browser_Test.Back();
            }
        }

        private void buttonGoTo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(boxSearch.Text))
            {
                Browser_Test.Address = boxSearch.Text;
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (Browser_Test != null)
            {
                Browser_Test.Reload();
            }
        }

        private void buttonNewTab_Click(object sender, RoutedEventArgs e)
        {
            AddTab("https://www.google.com");
        }

        private void AddTab(string url)
        {
            var tabItem = new TabItem();
            var webBrowser = new ChromiumWebBrowser();

            tabItem.Header = "NewTab";
            tabItem.Content = webBrowser;
            tabControl.Items.Add(tabItem);

            webBrowser.Address = url;
            webBrowser.FrameLoadEnd += ChromiumWebBrowser_OnFrameLoadEnd;
            Browser_Test = webBrowser;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTab = tabControl.SelectedItem as TabItem;
            var selectedBrowser = selectedTab?.Content as ChromiumWebBrowser;
            Browser_Test = selectedBrowser;
        }
    }
}
