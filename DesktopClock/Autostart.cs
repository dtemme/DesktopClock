using System;
using Microsoft.Win32;

namespace DesktopClock
{
    public sealed class Autostart : IDisposable
    {
        private const string REGISTRY_NAME = "DesktopClock";
        private readonly RegistryKey runFolder;

        public Autostart()
        {
            runFolder = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        }

        public void Add(string path)
        {
            if (!Exists())
                runFolder.SetValue(REGISTRY_NAME, path);
        }

        public void Remove()
        {
            if (Exists())
                runFolder.DeleteValue(REGISTRY_NAME, false);
        }

        public bool Exists() => runFolder.GetValue(REGISTRY_NAME) != null;

        public void Dispose()
        {
            runFolder.Dispose();
        }
    }
}
