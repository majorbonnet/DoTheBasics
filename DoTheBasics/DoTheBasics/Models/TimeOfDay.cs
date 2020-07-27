using System;
using System.Collections.Generic;
using System.Text;

namespace DoTheBasics.Models
{
    public class TimeOfDay
    {
        public TimeOfDay(int hour)
        {
            Hour = hour;
            Minute = 0;
        }

        public TimeOfDay(int hour, int minute)
        {
            Hour = hour;
            Minute = minute;
        }

        public int Hour { get; private set;}

        public int Minute { get; private set; }
    }
}
