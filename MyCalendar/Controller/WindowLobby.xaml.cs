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
using System.Text.RegularExpressions;
using MyCalendar.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyCalendar.Controller
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowLobby : Window
    {
        private readonly Regex _numRegex = new Regex("[0-9]+");

        /*
         * YYYY : Calendar's Year
         * MM : Calendar's Month
         * DD : Work's Day
         * HH : Work's Hour
         * mm : Work's minute
         * C : Calendar's 'did' Works
         * MC : Calendar's 'total' Works
         * WORK : Work's content
         */
        private readonly string _monthTitleStr = "[YYYY]. [MM] - [C] / [MC]";
        private readonly string _monthContextStr = " 가장 가까운 일정 : [YYYY]. [MM]. [DD] [TIME] > [WORK]";

        public WindowLobby()
        {
            InitializeComponent();
            initializeCalendars();
        }

        private void CloseProgram(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CreateNewCalendar(object sender, RoutedEventArgs e)
        {
            short year = short.Parse(Program_Input_Year.Text);
            short month = short.Parse(Program_Input_Month.Text);

            PushConsole($"{year}년 {month}월의 캘린더 파일 생성을 시작합니다.");
            if (CalendarManager.getInstance().CreateNewCalendar(year, month))
            {
                PushConsole("캘린더 파일 생성에 성공하였습니다.");
            }
            else
            {
                PushConsole("캘린더 파일 생성에 실패하였습니다.");
            }
        }

        private void PushConsole(String msg)
        {
            Program_Console.AppendText($"\n{msg}");
            Program_Console.ScrollToEnd();
        }

        private void CheckNumber(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumber(e.Text);
        }

        private bool IsNumber(string text)
        {
            return _numRegex.IsMatch(text);
        }

        /*
         *  Call calendar files
         */

        private void initializeCalendars()
        {
            string[] calendars = CalendarManager.getInstance().GetCalendars();
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int futureCount = 0;

            foreach (string calName in calendars)
            {
                string[] dataSplit = calName.Split('-');
                int dataYear = Int16.Parse(dataSplit[0].Split('(')[1]);
                int dataMonth = Int16.Parse(dataSplit[1].Split(')')[0]);
                
                if(year == dataYear && month == dataMonth)
                {
                    SetNowMonth(year, month);
                } 
                else
                {
                    if((dataYear == year && dataMonth > month) || (dataYear > year))
                    {
                        if(futureCount < 3)
                            SetFutureMonth(dataYear, dataMonth, futureCount++);
                    }
                }
            }
        }

        private void SetNowMonth(int year, int month)
        {
            JObject calData = CalendarManager.getInstance().GetCalendarData(year, month);
            UpdateMonthContext(ThisMonth_Context, ThisMonth_Context_desc, calData);
        }

        private void SetFutureMonth(int year, int month, int count)
        {
            JObject calData = CalendarManager.getInstance().GetCalendarData(year, month);
            if(count == 0)
            {
                UpdateMonthContext(NextMonth_Context_001, NextMonth_Context_001_desc, calData);
            }
            else if (count == 1)
            {
                UpdateMonthContext(NextMonth_Context_002, NextMonth_Context_002_desc, calData);
            }
            else if (count == 2)
            {
                UpdateMonthContext(NextMonth_Context_003, NextMonth_Context_003_desc, calData);
            }
        }

        private void UpdateMonthContext(Label title, Label desc, JObject calData)
        {
            // Analysis Works Data
            JObject works = (JObject)calData["Works"];
            Work work = null;
            int calDay = 1;
            int overWorks = 0;

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
                        break;
                    }
                    else overWorks++;
                }
            }

            // Update Title Label Content
            title.Content = _monthTitleStr
                .Replace("[YYYY]", calData["Year"].ToString())
                .Replace("[MM]", calData["Month"].ToString())
                .Replace("[C]", overWorks.ToString())
                .Replace("[MC]", calData["TotalWorks"].ToString());

            // Update Description Label Content
            if (work != null)
            {
                desc.Content = _monthContextStr
                    .Replace("[YYYY]", calData["Year"].ToString())
                    .Replace("[MM]", calData["Month"].ToString())
                    .Replace("[DD]", KeepTwoCharacters(calDay))
                    .Replace("[TIME]", work.GetStartTime(true))
                    .Replace("[WORK]", work.GetDescription());
            }
            else
            {
                desc.Content = _monthContextStr.Split(':')[0] + "> 예정된 일정이 없습니다.";
            }

        }

        private string KeepTwoCharacters(int val)
        {
            return val < 10 ? $"0{val}" : val.ToString();
        }

    }
}
