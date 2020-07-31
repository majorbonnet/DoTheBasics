using System;
using System.Collections.Generic;
using System.Text;

namespace DoTheBasics
{
    public interface INotificationManager
    {
        void ScheduleNotification(int goalId, string title, string message, DateTime notificationTime);

        void Cancel(int goalId);
    }
}
