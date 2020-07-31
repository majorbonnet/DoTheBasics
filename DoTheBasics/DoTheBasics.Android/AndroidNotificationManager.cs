using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using DoTheBasics.Droid;
using Java.Lang;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidNotificationManager))]
namespace DoTheBasics.Droid
{  
    public class AndroidNotificationManager : INotificationManager
    {
        private readonly DateTime _jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private bool channelInitialized = false;
        private NotificationManager manager;
  
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";

        public void Cancel(int goalId)
        {
            var intent = CreateIntent(goalId);
            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, goalId, intent, PendingIntentFlags.Immutable);

            var alarmManager = GetAlarmManager();
            alarmManager.Cancel(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(Application.Context);
            notificationManager.Cancel(goalId);
        }

        public void ScheduleNotification(int goalId, string title, string message, DateTime notificationTime)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            long repeatEveryDay = 1000 * 60 * 60 * 24;    
            long totalMilliSeconds = (long)(notificationTime.ToUniversalTime() - _jan1st1970).TotalMilliseconds;
            if (totalMilliSeconds < JavaSystem.CurrentTimeMillis())
            {
                totalMilliSeconds = totalMilliSeconds + repeatEveryDay;
            }

            var intent = CreateIntent(goalId);

            intent.PutExtra(Constants.NotificationTitleKey, title);
            intent.PutExtra(Constants.NotificationBodyKey, message);
            intent.PutExtra(Constants.NotificationIdKey, goalId);


            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, goalId, intent, PendingIntentFlags.Immutable);
            var alarmManager = GetAlarmManager();
            alarmManager.SetRepeating(AlarmType.RtcWakeup, totalMilliSeconds, repeatEveryDay, pendingIntent);
        }

        public static Intent GetLauncherActivity()
        {

            var packageName = Application.Context.PackageName;
            return Application.Context.PackageManager.GetLaunchIntentForPackage(packageName);
        }

        private Intent CreateIntent(int id)
        {
            return new Intent(Application.Context, typeof(ScheduledAlarmHandler));
        }
        private AlarmManager GetAlarmManager()
        {

            var alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            return alarmManager;
        }
        void CreateNotificationChannel()
        {
            manager = (NotificationManager)Application.Context.GetSystemService(Application.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(Constants.NotificationChannelId, channelNameJava, NotificationImportance.Max)
                {
                    Description = channelDescription
                };
                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }
    }
}