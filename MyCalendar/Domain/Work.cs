using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MyCalendar.Domain
{
    class Work
    {
        int _day, _startTime, _endTime;
        string _desc;

        public Work(int day, int startTime, int endTime, string description)
        {
            _day = day;
            _startTime = startTime;
            _endTime = endTime;
            _desc = description;
        }

        public Work(int day, JObject workData)
        {
            _day = day;
            _startTime = Int32.Parse(workData["StartTime"].ToString());
            _endTime = Int32.Parse(workData["EndTime"].ToString());
            _desc = workData["Description"].ToString();
        }

        public int GetDay()
        {
            return _day;
        }

        public int GetStartTime()
        {
            return _startTime;
        }

        public String GetStartTime(bool isStr)
        {
            return $"{GetHour(_startTime)}:{GetMinute(_startTime)}";
        }

        public int GetEndTime()
        {
            return _endTime;
        }

        public String GetEndTime(bool isStr)
        {
            return $"{GetHour(_endTime)}:{GetMinute(_endTime)}";
        }

        public String GetDescription()
        {
            return _desc;
        }

        public bool IsOver()
        {
            int day = DateTime.Now.Day;
            int hour = DateTime.Now.Hour;
            int min = DateTime.Now.Minute;
            int time = hour * 60 + min;

            return (day > _day) || (day == _day && time > _startTime);
        }

        private String GetHour(int val)
        {
            return KeepTwoCharacters(val/60);
        }

        private String GetMinute(int val)
        {
            return KeepTwoCharacters(val%60);
        }

        private String KeepTwoCharacters(int val)
        {
            return val < 10 ? $"0{val}" : val.ToString();
        }
    }
}
