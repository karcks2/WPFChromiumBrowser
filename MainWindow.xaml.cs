﻿using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
