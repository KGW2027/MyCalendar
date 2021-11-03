﻿using System;
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

namespace MyCalendar.Controller
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowLobby : Window
    {
        public WindowLobby()
        {
            FontFamilyMapCollection fonts = FontFamily.FamilyMaps;
            foreach(FontFamily font in Fonts.SystemFontFamilies)
            {
                Console.WriteLine(font.Source);
            }
            InitializeComponent();
        }

        private void CloseProgram(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
