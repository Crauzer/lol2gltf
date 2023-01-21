using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using System;

namespace lol2gltf.Helpers
{
    public static class NotificationHelper
    {
        private static WindowNotificationManager _notificationManager;

        public static void SetNotificationManager(Window host) =>
            _notificationManager = new WindowNotificationManager(host)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };

        public static void Show(Exception exception)
        {
            if (exception.InnerException is Exception innerException)
            {
                Show(
                    new Notification(
                        exception.Message,
                        innerException.Message,
                        type: NotificationType.Error,
                        expiration: TimeSpan.FromMilliseconds(-1)
                    )
                );
            }
            else
            {
                Show(
                    new Notification(
                        "Error",
                        exception.Message,
                        type: NotificationType.Error,
                        expiration: TimeSpan.FromMilliseconds(-1)
                    )
                );
            }
        }

        public static void ShowSuccess(string message)
        {
            Show(new Notification("Success", message, NotificationType.Success, TimeSpan.FromSeconds(2)));
        }

        public static void Show(INotification notification) => _notificationManager?.Show(notification);
    }
}
