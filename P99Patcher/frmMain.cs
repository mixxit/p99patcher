using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace P99Patcher
{    
    public partial class frmMain : Form
    {
        public string EQPath = "";

        public frmMain()
        {
            InitializeComponent();
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseUp);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseMove);

        }

        private Point mouseOffset;
        private bool isMouseDown = false;

        private void frmMain_MouseDown(object sender,
System.Windows.Forms.MouseEventArgs e)
        {
            int xOffset;
            int yOffset;
            if (e.Button == MouseButtons.Left)
            {
                xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
                yOffset = -e.Y -
            SystemInformation.CaptionHeight - SystemInformation.FrameBorderSize.Height;

                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }
        private void frmMain_MouseMove(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }

        private void frmMain_MouseUp(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }

        private void LoadSettings()
        {
            

            string path = Application.StartupPath + "\\settings.xml";
            try
            {
                XmlReader reader = XmlReader.Create(path);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name.ToString() == "path")
                        {
                            this.EQPath = reader.ReadString().ToString();
                        }
                    }
                }
            }
            catch
            {
                ConfigurePath();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimise_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnAccount_Click(object sender, EventArgs e)
        {
            string url = "http://www.eqemulator.org/forums/profile.php?do=loginserver";
            System.Diagnostics.Process.Start(url);
        }

        private void btnWeb_Click(object sender, EventArgs e)
        {
            string url = "http://www.project1999.org";
            System.Diagnostics.Process.Start(url);
        }

        private void ConfigurePath()
        {
            MessageBox.Show("Please locate your EQGame.exe in your Everquest folder");

            OpenFileDialog ofn = new OpenFileDialog();
            ofn.Filter = "EQGame.exe (*.exe)|*.exe";
            ofn.Title = "Find EQGame.exe";
            string FilePath = "";
            if (ofn.ShowDialog() == DialogResult.Cancel)
            {
                NoFile();
            }
            else
            {
                try
                {
                    FilePath = ofn.FileName;

                }
                catch (Exception)
                {
                    MessageBox.Show("Error opening file", "File Error",
                                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            if (FilePath != "")
            {
                if (DeleteXML())
                {
                    string oldpath = FilePath;
                    string newpath = oldpath.Replace("eqgame.exe", "eqhost.txt");
                    File.Delete(newpath);
                    File.Copy(Application.StartupPath + "\\eqhost.txt", newpath, true);
                    this.EQPath = oldpath;
                    SaveXML();
                }
                // close and pass
            }
            else
            {
                MessageBox.Show("Error: Blank filepath", "File Error",
                                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool DeleteXML()
        {
            string path = Application.StartupPath + "\\settings.xml";
            try
            {
                File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SaveXML()
        {

            string path = Application.StartupPath + "\\settings.xml";
            try
            {
                XmlTextWriter textWriter = new XmlTextWriter(path, null);
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("connect");

                textWriter.WriteStartElement("path");
                textWriter.WriteString(this.EQPath);
                textWriter.WriteEndElement();

                textWriter.WriteEndElement();

                textWriter.WriteEndDocument();
                textWriter.Close();
            }
            catch
            {
                MessageBox.Show("Error writing file, is it read only?");
            }

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void NoFile()
        {
            MessageBox.Show("Could not locate EQGame.exe, exiting.", "Missing EQ Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Application.Exit();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "eqgame.exe";
            string oldpath = this.EQPath;
            string justdir = oldpath.Replace("\\eqgame.exe", "");
            startInfo.WorkingDirectory = @justdir;
            startInfo.Arguments = "patchme";
            Process process;
            process = Process.Start(startInfo);
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "OptionsEditor.exe";
            string oldpath = this.EQPath;
            string justdir = oldpath.Replace("\\eqgame.exe", "");
            startInfo.WorkingDirectory = @justdir;
            Process process;
            process = Process.Start(startInfo);
        }
    }
}
