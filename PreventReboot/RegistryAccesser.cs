using Microsoft.Win32;
using System;
using System.ComponentModel;

namespace PreventReboot
{
    public class RegistryAccesser
    {
        private RegistryView registryView;

        public RegistryAccesser(RegistryView registryView = RegistryView.Default)
        {
            if (registryView == RegistryView.Default)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    this.registryView = RegistryView.Registry64;
                }
                else
                {
                    this.registryView = RegistryView.Registry32;
                }
            }
            else
            {
                this.registryView = registryView;
            }
        }

        public T? getValue<T>(RegistryHive hive, string subKey, string valueName)
            where T : struct
        {
            T? result = null;
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    object value = RegistryKey.OpenBaseKey(hive, this.registryView)?.OpenSubKey(subKey, false)?.GetValue(valueName);
                    result = (T)converter.ConvertTo(value, typeof(T));
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public void setValue<T>(RegistryHive hive, string subKey, string valueName, T value, RegistryValueKind kind)
            where T : struct
        {
            RegistryKey basekey = RegistryKey.OpenBaseKey(hive, this.registryView);
            RegistryKey subkey = basekey?.OpenSubKey(subKey, true);
            if (subkey == null)
            {
                subkey = basekey?.CreateSubKey(subKey, true);
            }
            subkey?.SetValue(valueName, value, kind);
        }

    }
}
