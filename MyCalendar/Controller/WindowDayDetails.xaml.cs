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
using Newtonsoft.Json.Linq;
using MyCalendar.Domain;

namespace MyCalendar.Controller
{
    /// <summary>
    /// WindowDayDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowDayDetails : Window
    {
        private readonly string _TITLE = "Day Info [ {YYYY} - {MM} - {DD} ]";

        private int _colorCount;
        private Button _enableInfo;


        public WindowDayDetails(int year, int month, int day)
        {
            _colorCount = 0;
            _enableInfo = null;

            InitializeComponent();

            string str = _TITLE.Replace("{YYYY}", year.ToString()).Replace("{MM}", KeepLength(month)).Replace("{DD}", KeepLength(day));
            Label_Title.Content = str;

            UpdateSchedules(year, month, day);
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

        private void HideInfo(object sender, RoutedEventArgs e)
        {
            ToggleInfoStatus(Visibility.Collapsed);
            if(_enableInfo != null)
            {
                UpdateInfo();
                _enableInfo = null;
            }
        }

        private void LoadInfo(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (_enableInfo != null)
            {
                int[] infos = ParseEnableInfo();
                UpdateInfo();
                UpdateSchedules(infos[0], infos[1], infos[2]);
            }

            _enableInfo = btn;

            int[] keys = ParseEnableInfo();
            Work work = CalendarManager.GetInstance().GetDayWorks(keys[0], keys[1], keys[2])[keys[3]];

            Label_Info_Starttime.Content = work.GetStartTime(true);
            Label_Info_Endtime.Content = work.GetEndTime(true);
            Label_Info_Description.Content = work.GetDescription();
            Textbox_Info_Memo.Text = work.GetMemo();
            ChangeStaredStatus(work.GetStared());
            ToggleInfoStatus(Visibility.Visible);
        }

        private void ToggleStarStatus(object sender, RoutedEventArgs e)
        {
            ChangeStaredStatus(!IsStared());
        }

        private void RemoveThisSchedule(object sender, RoutedEventArgs e)
        {
            int[] keys = ParseEnableInfo();
            CalendarManager.GetInstance().UpdateCalendar(keys[0], keys[1], keys[2], keys[3], null);
            UpdateSchedules(keys[0], keys[1], keys[2]);
        }

        /*
         * Non-event Methods
         */

        private string KeepLength(int val)
        {
            return val < 10 ? "0" + val : val.ToString();
        }

        private Brush GetColorNow()
        {
            Brush[] brushes = new Brush[]
            {
                new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0xBD)),
                new SolidColorBrush(Color.FromRgb(0xD2, 0xFF, 0xBD)),
                new SolidColorBrush(Color.FromRgb(0xF0, 0xF1, 0xFF)),
                new SolidColorBrush(Color.FromRgb(0xFF, 0xF7, 0xD6)),
                new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xC9)),
                new SolidColorBrush(Color.FromRgb(0xFF, 0xF4, 0xE3)),
                new SolidColorBrush(Color.FromRgb(0xFF, 0xE4, 0xBD)),
                new SolidColorBrush(Color.FromRgb(0xC9, 0xE9, 0xFF)),
                new SolidColorBrush(Color.FromRgb(0xF0, 0xE3, 0xFF))
            };

            if (_colorCount >= brushes.Length) _colorCount = 0;

            return brushes[_colorCount++];
        }

        private int[] ParseEnableInfo()
        {
            Button btn = _enableInfo;
            string data = btn.Name.Split('_')[2];
            int year = Int32.Parse(data.Split('Y')[0]);
            int month = Int32.Parse(data.Split('Y')[1].Split('M')[0]);
            int day = Int32.Parse(data.Split('M')[1].Split('D')[0]);
            int key = Int32.Parse(data.Split('D')[1]);

            return new int[] { year, month, day, key };
        }

        private void UpdateInfo()
        {
            int[] keys = ParseEnableInfo();
            Work work = CalendarManager.GetInstance().GetDayWorks(keys[0], keys[1], keys[2])[keys[3]];
            JObject jo = work.GetJObject();
            jo["IsStared"] = IsStared();
            jo["Memo"] = Textbox_Info_Memo.Text;

            CalendarManager.GetInstance().UpdateCalendar(keys[0], keys[1], keys[2], keys[3], jo);
        }

        private bool IsStared()
        {
            Brush star = Poly_Info_Star.Fill;
            Brush staredColor = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0x00));

            return star.ToString() == staredColor.ToString();
        }

        private Thickness CalculateMargin(int startY, int duration)
        {
            int leftTerm = 10;
            int gridWidth = 100;
            double windowHeight = Grid_Schedules.Height;
            double windowWidth = Grid_Schedules.Width;

            int line = 0;
            int bottom = (int)windowHeight - (startY + duration);

            while(Area.GetInstance().CheckClaim(startY, bottom, line) && line < 3)
            {
                line++;
            }

            int left = leftTerm + (line * 100);
            int right = (int)windowWidth - (gridWidth + left);

            Area.GetInstance().ClaimArea(startY, bottom, duration, line);

            return new Thickness(left, startY, right, bottom);
        }

        /*
         * WPF Object Methods
         */

        private void UpdateSchedules(int year, int month, int day)
        {
            Area.GetInstance().ResetClaim();
            InitializeGraph();
            InitializeWorks(year, month, day);
            ToggleInfoStatus(Visibility.Collapsed);
        }

        private void InitializeGraph()
        {
            Grid_Schedules.Children.Clear();

            Brush black = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            int time = 0;
            while(time++ <= 24)
            {
                Line line = new Line();
                line.X1 = 0; line.Y1 = 120 * time;
                line.X2 = 410; line.Y2 = 120 * time;
                line.StrokeThickness = 1.0D;
                line.StrokeDashArray = new DoubleCollection(new double[] { 1, 0, 1, 1 });
                line.Stroke = black;

                Label label = new Label();
                label.Content = $"{KeepLength(time)} : 00";
                label.Style = (Style)Application.Current.Resources["DefaultFont"];
                label.Foreground = black;
                label.FontSize = 20;
                label.Margin = new Thickness(410, -15 + (time*120), 0, -13);

                Grid_Schedules.Children.Add(line);
                Grid_Schedules.Children.Add(label);
            }
            InitializeCloseInfo();
        }

        private void InitializeWorks(int year, int month, int day)
        {
            List<Work> workList = CalendarManager.GetInstance().GetDayWorks(year, month, day);

            int gridWidth = 100;
            double windowHeight = Grid_Schedules.Height;
            double windowWidth = Grid_Schedules.Width;
            int key = 0;

            foreach(Work work in workList)
            {
                int startY = work.GetStartTime() * 2;
                int endY = work.GetEndTime() * 2;
                int duration = endY - startY;

                Grid workGrid = new Grid();
                workGrid.Height = duration; workGrid.Width = gridWidth;
                workGrid.Margin = CalculateMargin(startY, duration);

                Border border = new Border();
                border.CornerRadius = new CornerRadius(10.0D);
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                border.BorderThickness = new Thickness(1.0D);
                if (work.GetStared())
                {
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x7D, 0x00, 0xFF));
                    border.BorderThickness = new Thickness(2.0D);
                }
                border.Background = GetColorNow();

                TextBlock label = new TextBlock();
                label.Text = work.GetDescription();
                label.Style = (Style)Application.Current.Resources["ScheduleBlock"];

                Button btn = new Button();
                btn.Content = ""; btn.Width = gridWidth; btn.Height = duration;
                btn.Name = $"Btn_Schedule_{year}Y{month}M{day}D{key}";
                btn.Height = duration; btn.Width = gridWidth;
                btn.Opacity = 0.01f;
                btn.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                btn.Click += LoadInfo;

                border.Child = label;

                workGrid.Children.Add(border);
                workGrid.Children.Add(btn);

                Grid_Schedules.Children.Add(workGrid);

                key++;
            }
        }

        private void InitializeCloseInfo()
        {
            Button btn = new Button();
            btn.Content = ""; btn.Width = Grid_Schedules.Width; btn.Height = Grid_Schedules.Height;
            btn.Opacity = 0.01D;
            btn.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
            btn.Click += HideInfo;

            Grid_Schedules.Children.Add(btn);
        }

        private void ToggleInfoStatus(Visibility visible)
        {

            Label_Info_001.Visibility = visible;
            Label_Info_002.Visibility = visible;
            Label_Info_003.Visibility = visible;
            Label_Info_Starttime.Visibility = visible;
            Label_Info_Endtime.Visibility = visible;
            Label_Info_Description.Visibility = visible;
            Label_Info_Star.Visibility = visible;
            Label_Info_Delete.Visibility = visible;

            Textbox_Info_Memo.Visibility = visible;

            Poly_Info_Star.Visibility = visible;

            Border_Info_Delete.Visibility = visible;
            Border_Info_Star.Visibility = visible;

            Btn_Info_Star.Visibility = visible;
            Btn_Info_Delete.Visibility = visible;
        }

        private void ChangeStaredStatus(bool stared)
        {
            Brush star = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0x00));
            Brush unStar = new SolidColorBrush(Color.FromRgb(0xCB, 0xCE, 0xF2));

            Poly_Info_Star.Fill = stared ? star : unStar;
        }


    }
}
