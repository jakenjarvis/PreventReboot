using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreventReboot
{
    public static class RegistryCheckWriter<T>
        where T : struct
    {
        public static void set(RegistryAccesser accesser, RegistryHive hive, string subKey, string valueName, RegistryValueKind kind, T value)
        {
            T? oldValue = accesser.getValue<T>(hive, subKey, valueName);
            accesser.setValue<T>(hive, subKey, valueName, value, kind);
            T? newValue = accesser.getValue<T>(hive, subKey, valueName);

            Console.WriteLine($"{valueName,40}: {oldValue}\t--> {newValue}");
        }
    }
}
