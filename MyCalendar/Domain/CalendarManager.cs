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

        public static CalendarManager getInstance()
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


        private string ParseFileURL(short year, short month)
        {
            string monthStr = month < 10 ? $"0{month}" : month.ToString();
            return $"{_defaultAddress}/{_defaultFileName.Replace("[YYYY]", year.ToString()).Replace("[MM]",monthStr)}";
        }

    }
}
