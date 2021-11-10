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
using System.Windows.Shapes;

namespace MyCalendar.Controller
{
    /// <summary>
    /// WindowDayDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowDayDetails : Window
    {
        private readonly string _TITLE = "Day Info [ {YYYY} - {MM} - {DD} ]";

        public WindowDayDetails(int year, int month, int day)
        {

            InitializeComponent();

            string str = _TITLE.Replace("{YYYY}", year.ToString()).Replace("{MM}", KeepLength(month)).Replace("{DD}", KeepLength(day));
            Label_Title.Content = str;

            InitializeGraph();
        }

        /*
         * Event Methods
         */

        private void ShutdownWindow(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            e.Handled = true;
        }

        /*
         * Non-event Methods
         */

        private string KeepLength(int val)
        {
            return val < 10 ? "0" + val : val.ToString();
        }

        /*
         * Initialize Methods
         */

        private void InitializeGraph()
        {
            Border_Schedules.Children.Clear();
            Border_Schedules.RowDefinitions.Clear();


            int time = 0;
            while(time++ <= 24)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(60);

                Grid grid = InitLine(time);

                Border_Schedules.Children.Add(grid);
                Border_Schedules.RowDefinitions.Add(row);
                Grid.SetRow(grid, time - 1);
            }
        }

        private Grid InitLine(int time)
        {
            Grid grid = new Grid();
            grid.Height = 60; grid.Width = 300;

            Brush black = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            Line line = new Line();
            line.X1 = 0; line.Y1 = 60;
            line.X2 = 220; line.Y2 = 60;
            line.StrokeThickness = 1.0D;
            line.StrokeDashArray = new DoubleCollection(new double[] { 1, 0, 1, 1 });
            line.Stroke = black;

            Label label = new Label();
            label.Content = $"{KeepLength(time)} : 00";
            label.Style = (Style)Application.Current.Resources["DefaultFont"];
            label.Foreground = black;
            label.FontSize = 20;
            label.Margin = new Thickness(220, 38, 0, -13);

            grid.Children.Add(line);
            grid.Children.Add(label);

            return grid;
        }

        
    }
}
