using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DoTheBasics.Droid
{
    public static class Constants
    {
        public static readonly string NotificationTitleKey = "notificationTitle";
        public static readonly string NotificationBodyKey = "notificationBody";
        public static readonly string NotificationIdKey = "notificationId";
        public static readonly string NotificationChannelId = "DoTheBasicsChannel";
    }
}