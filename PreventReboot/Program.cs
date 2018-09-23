using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;

// https://natemcmaster.github.io/CommandLineUtils/
// https://github.com/dahall/TaskScheduler

namespace PreventReboot
{
    internal class Program
    {
        private static CommandLineApplication app = new CommandLineApplication(throwOnUnexpectedArg: false);
        private static bool debugMode = false;
        private static bool showHelpMessage = false;

        internal static int Main(string[] args)
        {
            int result = 0;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                app.FullName = "PreventReboot";
                app.Description = "This program prevents reboot during login by updating the ActiveTime of Windows10 on a regular basis.";
                app.VersionOptionFromAssemblyAttributes(Assembly.GetExecutingAssembly());

                app.HelpOption();
                app.ExtendedHelpText = "\r\nRemarks:\r\n"
                    + "  If no parameter is specified, this help is displayed.\r\n";

                var optionInstall = app.Option("-i|--install",
                    "Register the time setting task in the Task Manager.(Administrator authority required)",
                    CommandOptionType.NoValue);

                var optionUninstall = app.Option("-u|--uninstall",
                    "Delete the time setting task from the Task Manager.(Administrator authority required)",
                    CommandOptionType.NoValue);

                var optionSettings = app.Option("-s|--settings",
                    "Set the active time along the current time.(Administrator authority required)",
                    CommandOptionType.NoValue);

                var optionDebug = app.Option("-d|--debug",
                    "Outputs the contents of exception handling.(Usually not used)",
                    CommandOptionType.NoValue);

                var optionVersion = app.Option("-v|--version",
                    "Print the PreventReboot version number and exit.",
                    CommandOptionType.NoValue);

                app.OnExecute(() =>
                {
                    int ret = 0;
                    debugMode = optionDebug.HasValue();

                    var install = optionInstall.HasValue();
                    var uninstall = optionUninstall.HasValue();
                    var settings = optionSettings.HasValue();
                    var version = optionVersion.HasValue();


                    PreventRebootMain preventRebootMain = new PreventRebootMain();
                    if (install)
                    {
                        ret = preventRebootMain.Install();
                    }
                    else if (uninstall)
                    {
                        ret = preventRebootMain.Uninstall();
                    }
                    else if (settings)
                    {
                        ret = preventRebootMain.Settings();
                    }
                    else if(version)
                    {
                        app.ShowVersion();
                    }
                    else
                    {
                        ret = 0;
                        app.ShowHelp();
                        showHelpMessage = true;
                    }
                    return ret;
                });

                result = app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                result = 1;
                OutputException(e);
            }
            catch (Unauthorized​Access​Exception e)
            {
                result = 2;
                OutputException(e);
            }
            catch (InvalidOperationException e)
            {
                result = 3;
                OutputException(e);
            }
            catch (Exception e)
            {
                result = -1;
                OutputException(e);
            }
            finally
            {
                Environment.Exit(result);
            }
            return result;
        }

        public static string generateDefaultAppUserModelID()
        {
            // https://docs.microsoft.com/it-it/windows/desktop/shell/appids
            // CompanyName.ProductName.SubProduct.VersionInformation

            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemName = assembly.GetName();

            //AssemblyCompany
            string assemblyCompany = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute))).Company;
            //AssemblyProduct
            string assemblyProduct = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;
            //Version
            string major = assemName.Version.Major.ToString();
            string minor = assemName.Version.Minor.ToString();
            string build = assemName.Version.Build.ToString();
            string revision = assemName.Version.Revision.ToString();

            return $"{assemblyCompany}.{assemblyProduct}.{major}.{minor}.{build}.{revision}";
        }

        internal static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;
            OutputException(ex);
            Environment.Exit(-1);
        }

        internal static void OutputException(Exception e)
        {
            if (!showHelpMessage)
            {
                app.ShowHelp();
                showHelpMessage = true;
            }

            Console.ForegroundColor = ConsoleColor.DarkRed;
            try
            {
                TextWriter errorWriter = Console.Error;
                if (debugMode)
                {
                    errorWriter.WriteLine("Exception: " + e.ToString());
                }
                else
                {
                    errorWriter.WriteLine("Exception: " + e.Message);
                }
            }
            finally
            {
                Console.ResetColor();
            }
        }

    }
}
