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
using MyCalendar.Domain;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MyCalendar.Controller
{

    public partial class WindowMain : Window
    {
        private readonly Regex _numRegex = new Regex("[0-9]+");
        private Button _selectedButton;

        public WindowMain()
        {
            InitializeComponent();
            InitializeCalendarList();
        }

        /*
         * Event Methods
         */

        private void ShutdownWindow(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            e.Handled = true;
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CreateNewCalendar(object sender, RoutedEventArgs e)
        {
            short year = short.Parse(Input_Year.Text);
            short month = short.Parse(Input_Month.Text);

            bool success = CalendarManager.getInstance().CreateNewCalendar(year, month);
            InitializeCalendarList();
        }
        private void CheckNumber(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumber(e.Text);
        }

        private void ChangeViewCalendar(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;
            if(clicked.Name.StartsWith("Btn_Calendar_"))
            {
                if(_selectedButton != null)
                {
                    _selectedButton.Opacity = 0.01f;
                }
                clicked.Opacity = 0.35f;
                _selectedButton = clicked;

                string date = clicked.Name.Split('_')[2];
                int year = Int32.Parse(date.Split('Y')[0]);
                int month = Int32.Parse(date.Split('Y')[1].Split('M')[0]);

                LoadCalendar(year, month);

            }
        }

        /*
         * Non-Event Methods
         */

        private bool IsNumber(string str)
        {
            return _numRegex.IsMatch(str);
        }

        private string KeepTwoCharacters(int num)
        {
            return num < 10 ? $"0{num}" : num.ToString();
        }

        /*
         * Initiallize Methods
         */

        private void InitializeCalendarList()
        {
            Grid_Calendars.RowDefinitions.Clear();
            string[] calendars = CalendarManager.getInstance().GetCalendars();
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            int key = 0;

            foreach (string calName in calendars)
            {
                string[] dataSplit = calName.Split('-');
                int dataYear = Int16.Parse(dataSplit[0].Split('(')[1]);
                int dataMonth = Int16.Parse(dataSplit[1].Split(')')[0]);

                if(!Check_Past.IsChecked.Value)
                {
                    if (dataYear < year) continue; // 이미 연도가 지난 경우
                    if (dataYear == year && dataMonth < month) continue; // 같은 연도지만 월이 지난 경우
                }

                Console.WriteLine($"{dataYear} > {dataMonth} > {key}");
                AddCalendarToList(dataYear, dataMonth, key++);
            }
        }

        private string GetCloseWork(int year, int month)
        {
            JObject calData = CalendarManager.getInstance().GetCalendarData(year, month);
            JObject works = (JObject)calData["Works"];
            Work work = null;
            int calDay = 1;
            int overWorks = 0;
            bool foundWorkData = false;

            foreach (JProperty property in works.Properties())
            {
                calDay = Int32.Parse(property.Name);
                JArray calDayWork = (JArray)works[property.Name];
                foreach (Object obj in calDayWork)
                {
                    JObject calWorkData = (JObject)obj;
                    work = new Work(calDay, calWorkData);
                    if (!work.IsOver())
                    {
                        foundWorkData = true;
                        break;
                    }
                    else overWorks++;
                }
                if (foundWorkData) break;
            }

            if (work == null || !foundWorkData) return "예정된 일정이 없습니다.";

            return $"{KeepTwoCharacters(calDay)}일 {work.GetStartTime(true)} > {work.GetDescription()}";
        }

        private void AddCalendarToList(int year, int month, int row)
        {

            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(60.0D);

            Grid_Calendars.RowDefinitions.Add(rowDefinition);
            Grid grid = new Grid();
            grid.Height = 60; grid.Width = 225;

            Label title = new Label();
            title.Content = $"{year}. {month}.";
            title.Style = (Style)Application.Current.Resources["DefaultFont"];
            title.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            title.FontSize = 18;
            title.Margin = new Thickness(5, 4, 99, 28);

            Label description = new Label();
            description.Content = GetCloseWork(year, month);
            description.Style = (Style)Application.Current.Resources["DefaultFont"];
            description.Foreground = new SolidColorBrush(Color.FromRgb(0x7f, 0x7f, 0x7f));
            description.FontSize = 18;
            description.Margin = new Thickness(10, 23, 10, 0);

            Button btn = new Button();
            btn.Name = $"Btn_Calendar_{year}Y{KeepTwoCharacters(month)}M";
            btn.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            btn.Opacity = 0.01f;
            btn.Width = 225;
            btn.Height = 60;
            btn.Click += ChangeViewCalendar;

            Border decoBorder = new Border();
            decoBorder.CornerRadius = new CornerRadius(5, 5, 5, 5);
            decoBorder.Background = new SolidColorBrush(Color.FromRgb(0x7F, 0x7F, 0x7F));
            decoBorder.BorderBrush = null;
            decoBorder.Height = 5;
            decoBorder.Width = 215;
            decoBorder.Margin = new Thickness(5, 55, 0, 0);

            grid.Children.Add(title);
            grid.Children.Add(description);
            grid.Children.Add(btn);
            grid.Children.Add(decoBorder);

            Grid_Calendars.Children.Add(grid);
            Grid.SetColumn(grid, 0);
            Grid.SetRow(grid, row);

            Console.WriteLine($"{year}/{month} -> ADD COMPLETE");
        }

        private void LoadCalendar(int year, int month)
        {
            Label_Calendar_Title.Content = $"Calendar - [ {year} / {KeepTwoCharacters(month)} ] - ";
        }

    }
}
