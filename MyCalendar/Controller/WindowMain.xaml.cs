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
            if(IsCalendarButton(clicked))
            {
                if(_selectedButton != null)
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
            Console.WriteLine("Clicked -> " + clicked.Name);
            if(IsCalendarButton(clicked))
            {
                string date = clicked.Name.Split('_')[2];
                int year = Int32.Parse(date.Split('Y')[0]);
                int month = Int32.Parse(date.Split('Y')[1].Split('M')[0]);
                int day = Int32.Parse(date.Split('M')[1].Split('D')[0]);

                Console.WriteLine($"Clicked {year}-{month}-{day} Button");
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
            Grid_Calendars_List.RowDefinitions.Clear();
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

        private List<Work> GetCloseWorks(int year, int month)
        {
            List<Work> retVar = new List<Work>();
            JObject calData = CalendarManager.getInstance().GetCalendarData(year, month);
            if (calData == null) return retVar;
            JObject works = (JObject)calData["Works"];
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
                    Work work = new Work(calDay, calWorkData);
                    if (!work.IsOver())
                    {
                        if(!foundWorkData) foundWorkData = true;
                        retVar.Add(work);
                    }
                    else overWorks++;
                }
            }

            return retVar;
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

            int firstColumn = CalendarManager.getInstance().GetFirstDayOfMonth(year, month);
            int lastDay = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
            bool startDay = false;
            int day = 1;

            ResetCalendarGrid();

            for(int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    Grid grid = null;
                    if (row == 0 && col == firstColumn) startDay = true;
                    if(startDay)
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

        private bool IsCalendarButton(Button btn)
        {
            return btn.Name.StartsWith("Btn_Calendar");
        }

        private string CheckContentLength(string content, int maxLength)
        {
            if(content.Length > maxLength)
            {
                return content.Substring(0, maxLength - 1) + "...";
            }
            else
            {
                return content;
            }
        }

        private string GetClosestWorkString(List<Work> works)
        {
            if (works.Count == 0) return "예정된 일정이 없습니다.";
            return $"{KeepTwoCharacters(works[0].GetDay())}일 {works[0].GetStartTime(true)} > {works[0].GetDescription()}";
        }

        /*
         * WPF Object Create
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
            description.Content = CheckContentLength(GetClosestWorkString(GetCloseWorks(year, month)), 20);
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
        
            //<Grid Grid.Row="0" Grid.Column= "0" Height= "66" Width= "100" Background= "#B7C7E8" Margin= "1,0,0,1" >
            //    < Border BorderBrush= "#FFB0C0E0" BorderThickness= "1" HorizontalAlignment= "Left" Height= "66" VerticalAlignment= "Top" Width= "100" />
            //    < Label Content= "31" HorizontalAlignment= "Left" VerticalAlignment= "Top" Width= "28" FontSize= "16" FontWeight= "Bold" Height= "28" Margin= "-2,-5,0,0" />
            //    < Label Content= "08:00 기상" Style= "{StaticResource DefaultFont}" Foreground= "Black" FontSize= "14" Margin= "-3,16,2,23" />
            //    < Label Content= "09:00 Eng test" Style= "{StaticResource DefaultFont}" Foreground= "Black" FontSize= "14" Margin= "-3,31,2,10" />
            //    < Label Content= "10:00 무엇을 적을까" Style= "{StaticResource DefaultFont}" Foreground= "Black" FontSize= "14" Margin= "-2,46,1,-4" />
            //</ Grid >

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

            grid.Children.Add(dayDate);
            grid.Children.Add(btn);
            grid.Children.Add(border);

            List<Work> works = GetCloseWorks(date.Year, date.Month);
            if (works.Count == 0) return grid;

            int workCount = 0;

            foreach(Work work in works)
            {
                if (work.GetDay() != date.Day) continue;
                if (workCount++ > 1) break;

                Label dayWork = new Label();
                dayWork.Style = (Style)Application.Current.Resources["DefaultFont"];
                dayWork.Foreground = new SolidColorBrush(Color.FromRgb(0xB3, 0xAD, 0x7B));
                dayWork.FontSize = 18;
                dayWork.Content = $"{work.GetStartTime(true)} {CheckContentLength(work.GetDescription(), 5)}";
                dayWork.Margin = new Thickness(-3, 24, 0, 16);
                if(workCount == 2)
                {
                    dayWork.Margin = new Thickness(-3, 41, 0, 0);
                }
                grid.Children.Add(dayWork);
            }

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

            grid.Children.Add(dayDate);
            grid.Children.Add(btn);
            grid.Children.Add(border);

            List<Work> works = GetCloseWorks(date.Year, date.Month);
            if (works.Count == 0) return grid;

            int workCount = 0;

            foreach (Work work in works)
            {
                if (work.GetDay() != date.Day) continue;
                if (workCount++ > 1) break;

                Label dayWork = new Label();
                dayWork.Style = (Style)Application.Current.Resources["DefaultFont"];
                dayWork.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xF8, 0xBD));
                dayWork.FontSize = 18;
                dayWork.Content = $"{work.GetStartTime(true)} {CheckContentLength(work.GetDescription(), 5)}";
                dayWork.Margin = new Thickness(-3, 24, 0, 16);
                if (workCount == 2)
                {
                    dayWork.Margin = new Thickness(-3, 41, 0, 0);
                }
                grid.Children.Add(dayWork);
            }

            return grid;

        }

    }


}
