using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace PreventReboot
{
    public class PreventRebootMain
    {
        private const string D_PREVENT_REBOOT_TASK = "PreventRebootTask";

        public PreventRebootMain()
        {
        }

        private void withThrowExceptionIfPermissionDeneed()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                throw new Unauthorized​Access​Exception(
                    $"Cannot execute task with your current identity '{identity.Name}' permissions level.\r\n"
                    + "You likely need to run this application 'as administrator' even if you are using an administrator account.\r\n");
            }
        }

        public int Install()
        {
            int result = 0;
            Console.WriteLine("Start Install..");
            this.withThrowExceptionIfPermissionDeneed();

            if (isTargetTask())
            {
                throw new InvalidOperationException("Task has already been setup. Please uninstall first.");
            }

            using (TaskService taskService = new TaskService())
            {
                //string path = AppDomain.CurrentDomain.BaseDirectory;
                string exeFullPathFileName = Process.GetCurrentProcess().MainModule.FileName;
                //Console.WriteLine($"path       : {path}");
                //Console.WriteLine($"name       : {exeFullPathFileName}");

                // create shortcut
                ShortcutCreator shortcut = new ShortcutCreator(exeFullPathFileName, "-s");
                shortcut.AppUserModelID = Program.generateDefaultAppUserModelID();
                shortcut.create();

                // regist task scheduler
                TimeSpan span = TimeSpan.FromHours(1);
                Task task = taskService.Execute(exeFullPathFileName).WithArguments("-s").AtLogon().RepeatingEvery(span).AsTask(D_PREVENT_REBOOT_TASK);

                TaskDefinition definition = task.Definition;

                TaskPrincipal principal = definition.Principal;
                principal.RunLevel = TaskRunLevel.Highest;
                principal.UserId = "SYSTEM";

                TaskRegistrationInfo info = definition.RegistrationInfo;
                info.Author = "PreventReboot";
                info.Description = "This program prevents reboot during login by updating the ActiveTime of Windows10 on a regular basis.";

                TaskSettings settings = definition.Settings;
                settings.DisallowStartIfOnBatteries = false;
                settings.StopIfGoingOnBatteries = false;
                settings.RunOnlyIfIdle = false;
                settings.Hidden = true;
                settings.Enabled = true;
                settings.StartWhenAvailable = true;

                IdleSettings idle = settings.IdleSettings;
                idle.RestartOnIdle = false;
                idle.StopOnIdleEnd = false;

                task.RegisterChanges();
            }

            //new TaskService().AddTask("Memo", new DailyTrigger { DaysInterval = 3 }, new ExecAction("notepad.exe", null, null));

            return result;
        }

        public int Uninstall()
        {
            int result = 0;
            Console.WriteLine("Start Uninstall..");
            this.withThrowExceptionIfPermissionDeneed();

            // delete shortcut
            string exeFullPathFileName = Process.GetCurrentProcess().MainModule.FileName;
            ShortcutCreator shortcut = new ShortcutCreator(exeFullPathFileName, "-s");
            shortcut.delete();

            if (!isTargetTask())
            {
                throw new InvalidOperationException("Task does not exist. It has already been uninstalled.");
            }

            using (TaskService taskService = new TaskService())
            {
                taskService.RootFolder.DeleteTask(D_PREVENT_REBOOT_TASK);
            }

            return result;
        }

        public int Settings()
        {
            int result = 0;
            Console.WriteLine("Start Settings..");
            this.withThrowExceptionIfPermissionDeneed();

            // 時間取得と計算
            DateTime dtNow = DateTime.Now;
            DateTime dtBaseTime = dtNow.AddMinutes(31);     // 31分加算した日時を基準に計算する。
            DateTime dtStartTime = dtBaseTime.AddHours(-2);
            DateTime dtEndTime = dtStartTime.AddHours(12);

            Console.WriteLine($"Now       : {dtNow}");
            Console.WriteLine($"BaseTime  : {dtBaseTime} (add 31 min)");
            Console.WriteLine($"StartTime : {dtStartTime} (add -2 hour) --> {dtStartTime.Hour}");
            Console.WriteLine($"EndTime   : {dtEndTime} (add +12 hour) --> {dtEndTime.Hour}");

            // https://docs.microsoft.com/ja-jp/windows/deployment/update/waas-restart
            // https://docs.microsoft.com/ja-jp/security-updates/windowsupdateservices/18128158
            // https://blogs.technet.microsoft.com/jpwsus/2017/09/08/wecanstop-wu/

            RegistryAccesser accesser = new RegistryAccesser();

            Console.WriteLine(@"HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings");
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings",
                @"SetActiveHours", RegistryValueKind.DWord, 1);
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings",
                @"ActiveHoursStart", RegistryValueKind.DWord, dtStartTime.Hour);
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings",
                @"ActiveHoursEnd", RegistryValueKind.DWord, dtEndTime.Hour);

            // Policies
            Console.WriteLine(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate");
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate",
                @"SetActiveHours", RegistryValueKind.DWord, 1);
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate",
                @"ActiveHoursStart", RegistryValueKind.DWord, dtStartTime.Hour);
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate",
                @"ActiveHoursEnd", RegistryValueKind.DWord, dtEndTime.Hour);

            Console.WriteLine(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU");
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                @"AuOptions", RegistryValueKind.DWord, 4);
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                @"AlwaysAutoRebootAtScheduledTime", RegistryValueKind.DWord, 0);
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                @"NoAutoRebootWithLoggedOnUsers", RegistryValueKind.DWord, 1);

            Console.WriteLine(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\Installer");
            // 再起動マネージャーの使用を禁止する
            RegistryCheckWriter<int>.set(accesser, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Installer",
                @"DisableAutomaticApplicationShutdown", RegistryValueKind.DWord, 1);

            // Notification
            string appUserModelID = Program.generateDefaultAppUserModelID();
            var notification = new WindowsNotification(appUserModelID);
            notification.Show("TEST");

            return result;
        }

        private bool isTargetTask()
        {
            bool result = false;
            using (TaskService taskService = new TaskService())
            {
                result = (taskService.GetTask(D_PREVENT_REBOOT_TASK) != null);
            }
            return result;
        }

    }
}
