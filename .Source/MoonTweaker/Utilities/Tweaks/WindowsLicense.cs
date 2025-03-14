using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using MoonTweaker.Utilities.Control;

namespace MoonTweaker.Utilities.Tweaks
{
    internal sealed class WindowsLicense
    {
        internal static bool IsWindowsActivated = false;

        internal void LicenseStatus()
        {
            try
            {
                // Проверка статуса активации Windows
                using (var searcher = new ManagementObjectSearcher(
                    @"root\cimv2",
                    "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' AND LicenseStatus = 1"))
                {
                    foreach (var managementObj in searcher.Get())
                    {
                        IsWindowsActivated = (uint)managementObj["LicenseStatus"] == 1;
                        break; // Прерываем цикл после первого успешного результата
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке статуса активации: {ex.Message}");
                IsWindowsActivated = false;
            }
        }

        internal static async Task StartActivationAsync()
        {
            // Показываем уведомление о начале активации
            new ViewNotification().Show("", "warn", "activatewin_notification");

            try
            {
                // Асинхронное выполнение команды PowerShell
                await RunPowerShellCommandAsync("irm https://get.activated.win | iex");

                // Проверка статуса активации
                new WindowsLicense().LicenseStatus();

                // Отображение уведомления о результате активации
                if (IsWindowsActivated)
                {
                    new ViewNotification(300).Show("restart", "warn", "successactivate_notification");
                }
                else
                {
                    new ViewNotification(300).Show("", "warn", "notsuccessactivate_notification");
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine($"An error occurred: {ex.Message}");
                new ViewNotification(300).Show("", "error", $"activationerror_notification: {ex.Message}");
            }
        }

        private static async Task RunPowerShellCommandAsync(string command)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas" // Запуск от имени администратора
                };

                process.Start();

                // Асинхронное чтение вывода и ошибок
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit()); // Асинхронное ожидание завершения процесса

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"PowerShell Error: {error}");
                }

                Console.WriteLine(output);
            }
        }
    }
}