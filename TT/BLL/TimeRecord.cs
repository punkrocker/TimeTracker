using System.Data;
using Tstring.DataBase;
using TimeTracker.Util;

namespace TimeTracker.BLL
{
    public class TimeRecord
    {
        private static TimeRecord TimeRecorder;
        public static TimeRecord getTimeRecorder()
        {
            if (TimeRecorder == null)
                TimeRecorder = new TimeRecord();

            return TimeRecorder;
        }
    }
}
