using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CropCap
{
    public partial class Form1 : Form
    {

        public Configuration config;


        public Form1()
        {
            InitializeComponent();
            InitCustom();
        }

        public void InitCustom()
        {
            config = new Configuration();
            config.loadConfig();

            textBox1.Text = config.SaveLocation;
            checkedListBox1.SetItemChecked(0, config.CopyClipboard);
            checkedListBox2.SetItemChecked(0, config.Startup);

            setLists();
        }

        private void setLists()
        {
            String key = config.StringHotKey;

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

            if (key != null && !key.Equals(""))
            {
                if (key.Contains("Ctrl") && key.Contains("Alt"))
                    comboBox1.SelectedIndex = 2;
                else if (key.Contains("Ctrl") && key.Contains("Shift"))
                    comboBox1.SelectedIndex = 1;
                else if (key.Contains("Ctrl"))
                    comboBox1.SelectedIndex = 0;

                comboBox2.SelectedItem = key.Split('+')[1];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!textBox1.Text.Equals("") && !comboBox1.Text.Equals("") && !comboBox2.Text.Equals(""))
            {
                config.saveConfig(textBox1.Text, checkedListBox1.GetItemChecked(0), checkedListBox2.GetItemChecked(0), comboBox1.Text + "+" + comboBox2.Text);

                this.Opacity = 0;
                showSysTray();
                config.setHotKey(this);

                return;
            }

            MessageBox.Show("One or more of your boxes is empty, please check your Save Location and Hotkeys.");
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.BalloonTipText = "CropCap is still running in the background.";
                notifyIcon1.BalloonTipTitle = "App still running.";
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.Icon = this.Icon;
                notifyIcon1.ShowBalloonTip(3000);
                notifyIcon1.Visible = true;
                this.ShowInTaskbar = false;
            }

            else if (this.WindowState == FormWindowState.Normal)
            {
                notifyIcon1.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Opacity = 1;
            this.ShowInTaskbar = true;
            this.Visible = true;
            notifyIcon1.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (!textBox1.Text.Equals(""))
                fbd.SelectedPath = textBox1.Text;
            else
                fbd.SelectedPath = Environment.SpecialFolder.MyComputer.ToString();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox1.Text = fbd.SelectedPath;
        }

        public void showSysTray()
        {
            config.setHotKey(this);
            notifyIcon1.BalloonTipText = "CropCap is still running in the background.";
            notifyIcon1.BalloonTipTitle = "App still running.";

            ContextMenu menu = new System.Windows.Forms.ContextMenu();
            menu.MenuItems.Add("Open", (Object sender, EventArgs e) => 
            {
                this.Opacity = 1;
                this.ShowInTaskbar = true;
                this.Visible = true;
                notifyIcon1.Visible = false;
            });

            menu.MenuItems.Add("Exit", (Object sender, EventArgs e) =>
                {
                    this.Dispose();
                    this.Close();
                });

            notifyIcon1.ContextMenu = menu;

            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.Icon = this.Icon;
            notifyIcon1.ShowBalloonTip(3000);
            notifyIcon1.Visible = true;
            this.ShowInTaskbar = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!config.StringHotKey.Equals(""))
            {
                this.Visible = false;
                showSysTray();
                config.setHotKey(this);
            }

            base.OnLoad(e);
        }

    }
}
