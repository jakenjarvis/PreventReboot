using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;

// Taken from https://code.msdn.microsoft.com/windowsdesktop/sending-toast-notifications-71e230a2/
// Taken from https://github.com/Robertof/make-shortcut-with-appusermodelid

namespace PreventReboot
{
    internal enum STGM : long
    {
        STGM_READ = 0x00000000L,
        STGM_WRITE = 0x00000001L,
        STGM_READWRITE = 0x00000002L,
        STGM_SHARE_DENY_NONE = 0x00000040L,
        STGM_SHARE_DENY_READ = 0x00000030L,
        STGM_SHARE_DENY_WRITE = 0x00000020L,
        STGM_SHARE_EXCLUSIVE = 0x00000010L,
        STGM_PRIORITY = 0x00040000L,
        STGM_CREATE = 0x00001000L,
        STGM_CONVERT = 0x00020000L,
        STGM_FAILIFTHERE = 0x00000000L,
        STGM_DIRECT = 0x00000000L,
        STGM_TRANSACTED = 0x00010000L,
        STGM_NOSCRATCH = 0x00100000L,
        STGM_NOSNAPSHOT = 0x00200000L,
        STGM_SIMPLE = 0x08000000L,
        STGM_DIRECT_SWMR = 0x00400000L,
        STGM_DELETEONRELEASE = 0x04000000L,
    }

    internal static class ShellIIDGuid
    {
        internal const string IShellLinkW = "000214F9-0000-0000-C000-000000000046";
        internal const string CShellLink = "00021401-0000-0000-C000-000000000046";
        internal const string IPersistFile = "0000010b-0000-0000-C000-000000000046";
        internal const string IPropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
    }

    [ComImport,
    Guid(ShellIIDGuid.IShellLinkW),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellLinkW
    {
        UInt32 GetPath(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxPath,
            //ref _WIN32_FIND_DATAW pfd,
            IntPtr pfd,
            uint fFlags);
        UInt32 GetIDList(out IntPtr ppidl);
        UInt32 SetIDList(IntPtr pidl);
        UInt32 GetDescription(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxName);
        UInt32 SetDescription(
            [MarshalAs(UnmanagedType.LPWStr)] string pszName);
        UInt32 GetWorkingDirectory(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
            int cchMaxPath
            );
        UInt32 SetWorkingDirectory(
            [MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        UInt32 GetArguments(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
            int cchMaxPath);
        UInt32 SetArguments(
            [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        UInt32 GetHotKey(out short wHotKey);
        UInt32 SetHotKey(short wHotKey);
        UInt32 GetShowCmd(out uint iShowCmd);
        UInt32 SetShowCmd(uint iShowCmd);
        UInt32 GetIconLocation(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] out StringBuilder pszIconPath,
            int cchIconPath,
            out int iIcon);
        UInt32 SetIconLocation(
            [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
            int iIcon);
        UInt32 SetRelativePath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
            uint dwReserved);
        UInt32 Resolve(IntPtr hwnd, uint fFlags);
        UInt32 SetPath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport,
    Guid(ShellIIDGuid.IPersistFile),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPersistFile
    {
        UInt32 GetCurFile(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile
        );
        UInt32 IsDirty();
        UInt32 Load(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [MarshalAs(UnmanagedType.U4)] STGM dwMode);
        UInt32 Save(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            bool fRemember);
        UInt32 SaveCompleted(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
    }

    [ComImport]
    [Guid(ShellIIDGuid.IPropertyStore)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        UInt32 GetCount([Out] out uint propertyCount);
        UInt32 GetAt([In] uint propertyIndex, out PropertyKey key);
        UInt32 GetValue([In] ref PropertyKey key, [Out] PropVariant pv);
        UInt32 SetValue([In] ref PropertyKey key, [In] PropVariant pv);
        UInt32 Commit();
    }

    [ComImport,
    Guid(ShellIIDGuid.CShellLink),
    ClassInterface(ClassInterfaceType.None)]
    internal class CShellLink { }

    internal static class ResultChecker
    {
        internal static void VerifyAndThrow(UInt32 hresult)
        {
            if (hresult > 1)
            {
                throw new Exception("Failed with HRESULT: " + hresult.ToString("X"));
            }
        }
    }

    public class ShortcutCreator
    {
        public string ExePath { get; set; }
        public string Arguments { get; set; }
        public string AppUserModelID { get; set; }
        public string ShortcutPath { get; set; }

        public ShortcutCreator(string exePath, string arguments, string appUserModelID = "")
        {
            this.ExePath = exePath;
            this.Arguments = arguments;
            this.AppUserModelID = appUserModelID;
        }

        public string generateDefaultLinkPath()
        {
            string exeName = Path.GetFileNameWithoutExtension(this.ExePath);
            return this.generateLinkPath(exeName);
        }

        public string generateLinkPath(string title)
        {
            string programsStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            return Path.Combine(programsStartMenu, title + ".lnk");
        }

        public void delete(string shortcutPath = "")
        {
            if (!string.IsNullOrEmpty(shortcutPath))
            {
                if (!shortcutPath.Contains("\\"))
                {
                    this.ShortcutPath = this.generateLinkPath(shortcutPath);
                }
                else
                {
                    this.ShortcutPath = shortcutPath;
                }
            }

            if (string.IsNullOrEmpty(this.ShortcutPath))
            {
                this.ShortcutPath = this.generateDefaultLinkPath();
            }

            if (File.Exists(this.ShortcutPath))
            {
                File.Delete(this.ShortcutPath);
            }
        }

        public void create(string shortcutPath = "")
        {
            if (!string.IsNullOrEmpty(shortcutPath))
            {
                if(!shortcutPath.Contains("\\"))
                {
                    this.ShortcutPath = this.generateLinkPath(shortcutPath);
                }
                else
                {
                    this.ShortcutPath = shortcutPath;
                }
            }

            if (string.IsNullOrEmpty(this.ShortcutPath))
            {
                this.ShortcutPath = this.generateDefaultLinkPath();
            }

            // Find the path to the current executable
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ResultChecker.VerifyAndThrow(newShortcut.SetPath(this.ExePath));
            ResultChecker.VerifyAndThrow(newShortcut.SetArguments(this.Arguments));

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            if (!string.IsNullOrEmpty(this.AppUserModelID))
            {
                using (PropVariant appId = new PropVariant(this.AppUserModelID))
                {
                    ResultChecker.VerifyAndThrow(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                    ResultChecker.VerifyAndThrow(newShortcutProperties.Commit());
                }
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;
            ResultChecker.VerifyAndThrow(newShortcutSave.Save(this.ShortcutPath, true));
        }

    }
}
