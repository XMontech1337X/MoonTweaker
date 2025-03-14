using MoonTweaker.Utilities.Control;
using MoonTweaker.Utilities.Helpers;
using MoonTweaker.Utilities.Tweaks;
using MoonTweaker.Windows;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Controls;

namespace MoonTweaker.View
{
    public partial class MoreView : UserControl
    {
        public MoreView()
        {
            InitializeComponent();
        }

        private async void BtnLicenseWindows_ClickButton(object sender, EventArgs e)
        {
            if (WindowsLicense.IsWindowsActivated)
            {
                new ViewNotification().Show("", "info", "readyactivate_notification");
            }
            else
            {
                // Проверка доступности интернета
                if (!IsInternetAvailable())
                {
                    new ViewNotification().Show("", "error", "nointernet_notification");
                    return; // Прерываем выполнение, если интернет недоступен
                }

                // Запуск асинхронной активации
                await WindowsLicense.StartActivationAsync();
            }
        }

        // Метод для проверки доступности интернета
        private static bool IsInternetAvailable()
        {
            try
            {
                // Проверка доступности интернета через ping
                using (var ping = new Ping())
                {
                    var reply = ping.Send("www.google.com", 3000); // Таймаут 3 секунды
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                // Если ping не удался, проверяем доступность сетевых подключений
                return NetworkInterface.GetIsNetworkAvailable();
            }
        }

        private async void BtnRestorePoint_ClickButton(object sender, EventArgs e)
        {
            WaitingWindow waitingWindow = new WaitingWindow();
            waitingWindow.Show();
            new ViewNotification().Show("", "info", "createpoint_notification");
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { SystemMaintenance.CreateRestorePoint(); });
            waitingWindow.Close();
        }

        private void BtnRecoveyLaunch_ClickButton(object sender, EventArgs e) => SystemMaintenance.StartRecovery();

        private void BtnClear_ClickButton(object sender, EventArgs e) => new ClearingMemory().StartMemoryCleanup();

        private void BtnDisableDefrag_ClickButton(object sender, EventArgs e) => SystemMaintenance.DisableDefrag();

        private async void BtnDisableRecovery_ClickButton(object sender, EventArgs e)
        {
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate
            {
                try { SystemMaintenance.DisableRestorePoint(); } catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
            await backgroundQueue.QueueTask(delegate { new ViewNotification(300).Show("", "info", "disable_recovery_notification"); });
        }
    }
}
