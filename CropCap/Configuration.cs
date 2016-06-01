using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CropCap
{

    public class Configuration
    {
        public String SaveLocation { get; set; }
        public String StringHotKey { get; set; }
        public bool CopyClipboard { get; set; }
        public bool Startup { get; set; }
        public Hotkey Hotkey { get; set; }

        public Configuration()
        {
            SaveLocation = "";
            StringHotKey = "";
            CopyClipboard = true;
            Hotkey = new Hotkey();
        }

        public void saveConfig(String saveloc, bool cpyClip, bool startup, String hotkey)
        {
            try
            {
                if (File.Exists("config.ini"))
                    File.Delete("config.ini");

                using (StreamWriter sw = new StreamWriter(File.Open("config.ini", FileMode.Create)))
                {
                    sw.WriteLine(saveloc);
                    sw.WriteLine("" + cpyClip);
                    sw.WriteLine("" + startup);
                    sw.WriteLine(hotkey);
                }

                RegisterInStartup(startup);

                SaveLocation = saveloc;
                CopyClipboard = cpyClip;
                StringHotKey = hotkey;

            }
            catch (FileLoadException fe)
            {
                MessageBox.Show(fe.Message);
            }
        }

        public void loadConfig()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists("config.ini"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(File.Open("config.ini", FileMode.Open)))
                    {
                        SaveLocation = sr.ReadLine();
                        CopyClipboard = Boolean.Parse(sr.ReadLine());
                        Startup = Boolean.Parse(sr.ReadLine());
                        StringHotKey = sr.ReadLine();
                    }
                }
                catch (FileLoadException fe)
                {
                    MessageBox.Show(fe.Message);
                }
            }
        }

        public void RegisterInStartup(bool isChecked)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (isChecked)
            {
                registryKey.SetValue("CropCap", Application.ExecutablePath);
            }
            else
            {
                registryKey.DeleteValue("CropCap");
            }
        }

        public void setHotKey(Control control)
        {
            if (StringHotKey != null && !StringHotKey.Equals(""))
            {
                Hotkey = new Hotkey();

                if (StringHotKey.Contains("Ctrl"))
                    Hotkey.Control = true;
                if (StringHotKey.Contains("Shift"))
                    Hotkey.Shift = true;
                if (StringHotKey.Contains("Alt"))
                    Hotkey.Alt = true;

                String[] temp = StringHotKey.Split('+');

                Keys key = (Keys)System.Enum.Parse(typeof(Keys), StringHotKey.Split('+')[1]);
                Hotkey.KeyCode = key;
                ScreenShot ss = new ScreenShot(this);
                Hotkey.Pressed += delegate
                {
                    if (ss.form == null || ss.form.IsDisposed)
                    {

                        ss = new ScreenShot(this);
                        ss.TakeScreenShot();

                    }
                    //MessageBox.Show("Hotkey Successful!");

                };

                Hotkey.Register(control);

            }
        }

    }
}
