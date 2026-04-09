using System;

namespace Brew.Core.Services
{
    /// <summary>
    /// Platform notification scheduler that wraps Unity Mobile Notifications.
    /// Compile-guarded for iOS and Android; returns a safe no-op on unsupported
    /// platforms via <see cref="NullNotificationScheduler"/>.
    /// </summary>
    public sealed class UnityNotificationScheduler : INotificationScheduler
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private bool _channelRegistered;

        private void EnsureChannel()
        {
            if (_channelRegistered) return;
            var channel = new Unity.Notifications.Android.AndroidNotificationChannel
            {
                Id = "brew_default",
                Name = "Brew",
                Description = "Brew game notifications",
                Importance = Unity.Notifications.Android.Importance.Default
            };
            Unity.Notifications.Android.AndroidNotificationCenter.RegisterNotificationChannel(channel);
            _channelRegistered = true;
        }

        public void Schedule(string id, string title, string body, DateTime fireTime)
        {
            EnsureChannel();
            Cancel(id);
            var notification = new Unity.Notifications.Android.AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = fireTime,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large"
            };
            Unity.Notifications.Android.AndroidNotificationCenter.SendNotificationWithExplicitID(
                notification, "brew_default", int.Parse(id.GetHashCode().ToString("X"), System.Globalization.NumberStyles.HexNumber));
        }

        public void Cancel(string id)
        {
            Unity.Notifications.Android.AndroidNotificationCenter.CancelNotification(
                id.GetHashCode());
        }

        public void CancelAll()
        {
            Unity.Notifications.Android.AndroidNotificationCenter.CancelAllNotifications();
        }

        public bool HasPermission()
        {
            var status = Unity.Notifications.Android.AndroidNotificationCenter.UserPermissionToPost;
            return status == Unity.Notifications.Android.PermissionStatus.Allowed;
        }

#elif UNITY_IOS && !UNITY_EDITOR
        public void Schedule(string id, string title, string body, DateTime fireTime)
        {
            Cancel(id);
            var timeTrigger = new Unity.Notifications.iOS.iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = fireTime - DateTime.Now,
                Repeats = false
            };
            var notification = new Unity.Notifications.iOS.iOSNotification
            {
                Identifier = id,
                Title = title,
                Body = body,
                ShowInForeground = false,
                Trigger = timeTrigger
            };
            Unity.Notifications.iOS.iOSNotificationCenter.ScheduleNotification(notification);
        }

        public void Cancel(string id)
        {
            Unity.Notifications.iOS.iOSNotificationCenter.RemoveScheduledNotification(id);
        }

        public void CancelAll()
        {
            Unity.Notifications.iOS.iOSNotificationCenter.RemoveAllScheduledNotifications();
        }

        public bool HasPermission()
        {
            return Unity.Notifications.iOS.iOSNotificationCenter.GetNotificationSettings()
                .AuthorizationStatus == Unity.Notifications.iOS.AuthorizationStatus.Authorized;
        }

#else
        public void Schedule(string id, string title, string body, DateTime fireTime) { }
        public void Cancel(string id) { }
        public void CancelAll() { }
        public bool HasPermission() => false;
#endif
    }
}
