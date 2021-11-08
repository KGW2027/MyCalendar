using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyCalendar.Domain
{
    class CalendarManager
    {
        private static CalendarManager instance;

        public static CalendarManager GetInstance()
        {
            if(instance == null)
            {
                instance = new CalendarManager();
            }

            return instance;
        }

        private readonly string _defaultAddress = "./Calendars";
        private readonly string _defaultFileName = "Calendar([YYYY]-[MM]).json";

        public CalendarManager()
        {
            Directory.CreateDirectory(_defaultAddress);
        }

        public bool CreateNewCalendar(short year, short month)
        {
            string fileURL = ParseFileURL(year, month);
            if(File.Exists(fileURL))
            {
                AlertManager.getInstance().ShowAlert(AlertType.ERROR_ALREADY_EXISTS, new object[] { year, month });
                return false;
            }

            JObject calData = new JObject();
            calData["Year"] = year.ToString();
            calData["Month"] = month < 10 ? $"0{month}" : month.ToString();
            calData["TotalWorks"] = 0;
            calData["Works"] = new JObject(); // { 1(day) : [ {StartTime:18:00, EndTime:20:00, Description: Game Making}, ...Works], ...Days}

            File.WriteAllText(fileURL, calData.ToString());
            return true;
        }

        public string[] GetCalendars()
        {
            return Directory.GetFiles(_defaultAddress);
        }

        public JObject GetCalendarData(int year, int month)
        {
            string fileURL = ParseFileURL((short) year, (short) month);
            if(!File.Exists(fileURL))
            {
                AlertManager.getInstance().ShowAlert(AlertType.ERROR_FILE_NOT_EXISTS, new object[] { year, month });
                return null;
            }

            return JObject.Parse(File.ReadAllText(fileURL));
        }

        public void AddCalendar(int year, int month, int day, Work work)
        {
            string fileURL = ParseFileURL((short)year, (short)month);
            JObject calData = GetCalendarData(year, month);

            JObject works = calData["Works"] as JObject;
            JArray dayWorks = new JArray();
            if (works.ContainsKey(day.ToString())) dayWorks = works[day.ToString()] as JArray;

            JObject workData = work.GetJObject();
            dayWorks.Add(workData);
            calData["Works"][day.ToString()] = SortDailyWorks(dayWorks);

            File.WriteAllText(fileURL, calData.ToString());
            
        }

        public int GetFirstDayOfMonth(int year, int month)
        {
            DayOfWeek dayOfWeek = new DateTime(year, month, 1).DayOfWeek;
            return (int)dayOfWeek;
        }

        private JArray SortDailyWorks(JArray ja)
        {
            JArray sortResult = new JArray();
            sortResult.Add(ja[0]);

            /*
             * Insert Sorting
             * ja[i(++)]를 sortResult[j(--)]와 비교하면서 sortResult[j] < ja[i]인 지점에서 j+1에 ja[i]를 삽입
             */
            for (int i = 1; i < ja.Count; i++) 
            {
                int j = i - 1;
                while(j >= 0 && Int32.Parse(sortResult[j]["StartTime"].ToString()) > Int32.Parse(ja[i]["StartTime"].ToString()))
                {
                    j--;
                }
                sortResult.Insert(j + 1, ja[i]);
            }

            return sortResult;
        }

        public Work GetClosestWorks(int year, int month)
        {
            JObject calData = CalendarManager.GetInstance().GetCalendarData(year, month);
            JObject works = calData["Works"] as JObject;

            foreach (JProperty property in works.Properties())
            {
                int calDay = Int32.Parse(property.Name);
                JArray calDayWork = works[property.Name] as JArray;
                foreach (Object obj in calDayWork)
                {
                    JObject calWorkData = obj as JObject;
                    Work work = new Work(calDay, calWorkData);
                    if (!work.IsOver())
                    {
                        return work;
                    }
                }
            }

            return null;
        }

        public List<Work> GetDayWorks(int year, int month, int day)
        {
            List<Work> retVar = new List<Work>(); JObject calData = CalendarManager.GetInstance().GetCalendarData(year, month);
            if (calData == null || !(calData["Works"] as JObject).ContainsKey(day.ToString())) return retVar;
            JArray works = calData["Works"][day.ToString()] as JArray;
            foreach(JToken work in works)
            {
                int startTime = Int32.Parse(work["StartTime"].ToString());
                int endTime = Int32.Parse(work["EndTime"].ToString());
                string desc = work["Description"].ToString();

                retVar.Add(new Work(day, startTime, endTime, desc));
            }

            return retVar;
        }



        private string ParseFileURL(short year, short month)
        {
            string monthStr = month < 10 ? $"0{month}" : month.ToString();
            return $"{_defaultAddress}/{_defaultFileName.Replace("[YYYY]", year.ToString()).Replace("[MM]",monthStr)}";
        }

    }
}
