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
        private int year;
        private int month;

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

            bool success = CalendarManager.GetInstance().CreateNewCalendar(year, month);
            InitializeCalendarList();
        }
        private void CheckNumber(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumber(e.Text);
        }

        private void ChangeViewCalendar(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;
            if (IsCalendarButton(clicked))
            {
                if (_selectedButton != null)
                {
                    _selectedButton.Opacity = 0.01f;
                }
                clicked.Opacity = 0.35f;
                _selectedButton = clicked;

                string date = clicked.Name.Split('_')[3];
                int year = Int32.Parse(date.Split('Y')[0]);
                int month = Int32.Parse(date.Split('Y')[1].Split('M')[0]);

                LoadCalendar(year, month);

            }
        }

        private void OpenDayCalendarDetails(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;
            if (IsCalendarButton(clicked))
            {
                string date = clicked.Name.Split('_')[2];
                int year = Int32.Parse(date.Split('Y')[0]);
                int month = Int32.Parse(date.Split('Y')[1].Split('M')[0]);
                int day = Int32.Parse(date.Split('M')[1].Split('D')[0]);

                Console.WriteLine($"Clicked {year}-{month}-{day} Button");
            }
        }

        private void AddCalendar(object sender, RoutedEventArgs e)
        {
            if (year == null || month == null) return;

            int startTime = Int32.Parse(TB_Add_Calendar_Start_Hour.Text) * 60 + Int32.Parse(TB_Add_Calendar_Start_Minute.Text);
            int endTime = Int32.Parse(TB_Add_Calendar_End_Hour.Text) * 60 + Int32.Parse(TB_Add_Calendar_End_Minute.Text);
            string description = TB_Add_Calendar_Description.Text;

            if (CB_Add_Calendar_Mode.IsChecked.Value)
            {
                int day = Int32.Parse(TB_Add_Calendar_Day.Text);
                CalendarManager.GetInstance().AddCalendar(this.year, this.month, day, new Work(day, startTime, endTime, description));
            }
            else
            {
                List<int> targets = GetTargetAddDays(year, month, GetSelectedAddDays());
                foreach (int day in targets)
                {
                    CalendarManager.GetInstance().AddCalendar(this.year, this.month, day, new Work(day, startTime, endTime, description));
                }
            }

            LoadCalendar(year, month);
        }

        private void ChangeAddCalendarCheckbox(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;

            CB_Add_Calendar_Sunday.Visibility = box.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
            CB_Add_Calendar_Monday.Visibility = box.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
            CB_Add_Calendar_Tuesday.Visibility = box.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
            CB_Add_Calendar_Wednsday.Visibility = box.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
            CB_Add_Calendar_Thursday.Visibility = box.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
            CB_Add_Calendar_Friday.Visibility = box.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
            CB_Add_Calendar_Saturday.Visibility = box.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;

            TB_Add_Calendar_Day.Visibility = box.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            TB_Add_Calendar_Day_Label.Visibility = box.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;


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

        private bool IsCalendarButton(Button btn)
        {
            return btn.Name.StartsWith("Btn_Calendar");
        }

        private string CheckContentLength(string content, int maxLength)
        {
            if (content.Length > maxLength)
            {
                return content.Substring(0, maxLength - 1) + "...";
            }
            else
            {
                return content;
            }
        }

        private List<int> GetSelectedAddDays()
        {
            List<int> days = new List<int>();

            if (CB_Add_Calendar_Sunday.IsChecked.Value) days.Add(0);
            if (CB_Add_Calendar_Monday.IsChecked.Value) days.Add(1);
            if (CB_Add_Calendar_Tuesday.IsChecked.Value) days.Add(2);
            if (CB_Add_Calendar_Wednsday.IsChecked.Value) days.Add(3);
            if (CB_Add_Calendar_Thursday.IsChecked.Value) days.Add(4);
            if (CB_Add_Calendar_Friday.IsChecked.Value) days.Add(5);
            if (CB_Add_Calendar_Saturday.IsChecked.Value) days.Add(6);

            return days;
        }

        private List<int> GetTargetAddDays(int year, int month, List<int> selected)
        {
            List<int> days = new List<int>();
            DateTime date = new DateTime(year, month, 1);

            while (date.Month == month)
            {
                if (selected.Contains((int)date.DayOfWeek)) days.Add(date.Day);
                date = date.AddDays(1);
            }

            return days;
        }

        /*
         * Object Related Methods
         */

        private void InitializeCalendarList()
        {
            Grid_Calendars_List.Children.Clear();
            Grid_Calendars_List.RowDefinitions.Clear();
            string[] calendars = CalendarManager.GetInstance().GetCalendars();
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            int key = 0;

            foreach (string calName in calendars)
            {
                string[] dataSplit = calName.Split('-');
                int dataYear = Int16.Parse(dataSplit[0].Split('(')[1]);
                int dataMonth = Int16.Parse(dataSplit[1].Split(')')[0]);

                if (!Check_Past.IsChecked.Value)
                {
                    if (dataYear < year) continue; // 이미 연도가 지난 경우
                    if (dataYear == year && dataMonth < month) continue; // 같은 연도지만 월이 지난 경우
                }

                Console.WriteLine($"{dataYear} > {dataMonth} > {key}");
                AddCalendarToList(dataYear, dataMonth, key++);
            }
        }

        private void AddCalendarToList(int year, int month, int row)
        {

            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(60.0D);

            Grid_Calendars_List.RowDefinitions.Add(rowDefinition);
            Grid grid = CreateCalendarListData(year, month);

            Grid_Calendars_List.Children.Add(grid);
            Grid.SetColumn(grid, 0);
            Grid.SetRow(grid, row);

            Console.WriteLine($"{year}/{month} -> ADD COMPLETE");
        }

        private void LoadCalendar(int year, int month)
        {
            Label_Calendar_Title.Content = $"Calendar - [ {year} / {KeepTwoCharacters(month)} ] - ";
            this.year = year;
            this.month = month;

            int firstColumn = CalendarManager.GetInstance().GetFirstDayOfMonth(year, month);
            int lastDay = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
            bool startDay = false;
            int day = 1;

            ResetCalendarGrid();

            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    Grid grid = null;
                    if (row == 0 && col == firstColumn) startDay = true;
                    if (startDay)
                    {
                        grid = CreateDate(new DateTime(year, month, day++));
                        if (day > lastDay) startDay = false;
                    }
                    else
                    {
                        if (day == 1) grid = CreateDateNotTarget(new DateTime(year, month, 1).AddDays(col - firstColumn)); // Previous Month
                        else grid = CreateDateNotTarget(new DateTime(year, month, 1).AddDays(day++).AddDays(-1)); // Next Month
                    }
                    Grid.SetRow(grid, row);
                    Grid.SetColumn(grid, col);
                    Grid_Calendar.Children.Add(grid);

                }
            }

        }

        private void ResetCalendarGrid()
        {
            Grid_Calendar.Children.Clear();
            Grid_Calendar.RowDefinitions.Clear();
            Grid_Calendar.ColumnDefinitions.Clear();

            int height = 66;
            int width = 100;

            for (int row = 0; row < 5; row++)
            {
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(height);

                Grid_Calendar.RowDefinitions.Add(rowDef);
            }

            for (int col = 0; col < 7; col++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                colDef.Width = new GridLength(width);

                Grid_Calendar.ColumnDefinitions.Add(colDef);
            }
        }

        /*
         * Object Make Methods
         */

        private Grid CreateCalendarListData(int year, int month)
        {
            Grid grid = new Grid();
            grid.Height = 60; grid.Width = 225;

            Label title = new Label();
            title.Content = $"{year}. {month}.";
            title.Style = (Style)Application.Current.Resources["DefaultFont"];
            title.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            title.FontSize = 18;
            title.Margin = new Thickness(5, 4, 99, 28);

            Label description = new Label();
            Work closest = CalendarManager.GetInstance().GetClosestWorks(year, month);
            string message = closest == null
                ? "예정된 일정이 없습니다."
                : $"{KeepTwoCharacters(closest.GetDay())}일 {closest.GetStartTime(true)} > {closest.GetDescription()}";
            description.Content = CheckContentLength(message, 20);
            description.Style = (Style)Application.Current.Resources["DefaultFont"];
            description.Foreground = new SolidColorBrush(Color.FromRgb(0x7f, 0x7f, 0x7f));
            description.FontSize = 18;
            description.Margin = new Thickness(10, 23, 10, 0);

            Button btn = new Button();
            btn.Name = $"Btn_Calendar_List_{year}Y{KeepTwoCharacters(month)}M";
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

            return grid;
        }

        private Grid CreateDateNotTarget(DateTime date)
        {
            Grid grid = new Grid();
            grid.Height = 66; grid.Width = 100;
            grid.Margin = new Thickness(1, 0, 0, 1);
            grid.Background = new SolidColorBrush(Color.FromRgb(0xB7, 0xC7, 0xE8));

            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            border.HorizontalAlignment = HorizontalAlignment.Left; border.VerticalAlignment = VerticalAlignment.Top;
            border.Height = 66; border.Width = 100;

            Label dayDate = new Label();
            dayDate.Content = date.Day;
            dayDate.Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            dayDate.Height = 31; dayDate.Width = 31;
            dayDate.HorizontalAlignment = HorizontalAlignment.Left; dayDate.VerticalAlignment = VerticalAlignment.Top;
            dayDate.FontSize = 19; dayDate.FontWeight = FontWeights.Bold;
            dayDate.Margin = new Thickness(-2, -5, 0, 0);

            Button btn = new Button();
            btn.Name = $"Btn_Calendar_{date.Year}Y{date.Month}M{date.Day}D";
            btn.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            btn.Opacity = 0.01f;
            btn.Width = 100;
            btn.Height = 66;
            btn.Click += OpenDayCalendarDetails;


            List<Work> works = CalendarManager.GetInstance().GetDayWorks(date.Year, date.Month, date.Day);
            if (works.Count > 0)
            {
                int workCount = 0;

                foreach (Work work in works)
                {
                    if (work.GetDay() != date.Day) continue;
                    if (workCount++ > 1) break;

                    Label dayWork = new Label();
                    dayWork.Style = (Style)Application.Current.Resources["DefaultFont"];
                    dayWork.Foreground = new SolidColorBrush(Color.FromRgb(0xB3, 0xAD, 0x7B));
                    dayWork.FontSize = 18;
                    dayWork.Content = $"{work.GetStartTime(true)} {CheckContentLength(work.GetDescription(), 5)}";
                    dayWork.Margin = new Thickness(-3, 24, 0, 16);
                    if (workCount == 2)
                    {
                        dayWork.Margin = new Thickness(-3, 41, 0, 0);
                    }
                    grid.Children.Add(dayWork);
                }
            }

            grid.Children.Add(dayDate);
            grid.Children.Add(btn);
            grid.Children.Add(border);

            return grid;
        }

        private Grid CreateDate(DateTime date)
        {
            Grid grid = new Grid();
            grid.Height = 66; grid.Width = 100;
            grid.Margin = new Thickness(1, 0, 0, 1);
            grid.Background = new SolidColorBrush(Color.FromRgb(0xC7, 0xD7, 0xF8));

            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            border.HorizontalAlignment = HorizontalAlignment.Left; border.VerticalAlignment = VerticalAlignment.Top;
            border.Height = 66; border.Width = 100;

            Label dayDate = new Label();
            dayDate.Content = date.Day;
            dayDate.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
            dayDate.Height = 31; dayDate.Width = 31;
            dayDate.HorizontalAlignment = HorizontalAlignment.Left; dayDate.VerticalAlignment = VerticalAlignment.Top;
            dayDate.FontSize = 19; dayDate.FontWeight = FontWeights.Bold;
            dayDate.Margin = new Thickness(-2, -5, 0, 0);

            Button btn = new Button();
            btn.Name = $"Btn_Calendar_{date.Year}Y{date.Month}M{date.Day}D";
            btn.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            btn.Opacity = 0.01f;
            btn.Width = 100;
            btn.Height = 66;
            btn.Click += OpenDayCalendarDetails;


            List<Work> works = CalendarManager.GetInstance().GetDayWorks(date.Year, date.Month, date.Day);
            if (works.Count > 0)
            {
                int workCount = 0;

                foreach (Work work in works)
                {
                    if (work.GetDay() != date.Day) continue;
                    if (workCount++ > 1) break;

                    Label dayWork = new Label();
                    dayWork.Style = (Style)Application.Current.Resources["DefaultFont"];
                    dayWork.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x0D));
                    dayWork.FontSize = 18;
                    dayWork.Content = $"{work.GetStartTime(true)} {CheckContentLength(work.GetDescription(), 5)}";
                    dayWork.Margin = new Thickness(-3, 24, 0, 16);
                    if (workCount == 2)
                    {
                        dayWork.Margin = new Thickness(-3, 41, 0, 0);
                    }
                    grid.Children.Add(dayWork);
                }
            }

            grid.Children.Add(dayDate);
            grid.Children.Add(border);
            grid.Children.Add(btn);


            return grid;

        }

    }

}
