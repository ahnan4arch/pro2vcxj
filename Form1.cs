using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace pro2vcxj
{
    public partial class pro2vcxj : Form
    {
        private int files_remain = 0;
        private List<string> files_to_process = new List<string>();
        public pro2vcxj()
        {
            InitializeComponent();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            textBox1.AppendText(String.Join("\r\n", files) + "\r\n");
            files_to_process.AddRange(files);
            files_remain += files.Count();
            handle_pro();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "bat files|*.bat";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog.FileName;
                Properties.Settings.Default.vcDevCmd = textBox2.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                textBox2.Text = "";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "exe files|*.exe";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog.FileName;
                Properties.Settings.Default.qmake = textBox3.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                textBox3.Text = "";
            }
        }

        private void handle_pro()
        {
            if (files_remain > 0)
            {

                string filename = files_to_process[0];

                string pro = Path.GetFileName(filename);

                string proFileName = Path.GetFileNameWithoutExtension(filename);

                string proFileDir = System.IO.Directory.GetParent(filename).ToString();

                string env = "call " + (char)34 + textBox2.Text + (char)34;

                string cmd = (char)34 + textBox3.Text + (char)34 + " -t " + " vcapp " + " -o " + (char)34 + pro.Replace(".pro", ".vcxproj") + (char)34 + " " + (char)34 + pro + (char)34;

                if(MessageBox.Show("是单击确定否则点取消", (char)34 + pro + (char)34 + " 是vcapp工程", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign) == DialogResult.Cancel)
                {
                    cmd = cmd.Replace("vcapp", "vclib");
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.WorkingDirectory = proFileDir;
                processStartInfo.FileName = "cmd.exe";
                processStartInfo.Arguments = "/K";
                processStartInfo.UseShellExecute = false;
                processStartInfo.ErrorDialog = false;
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;

                processStartInfo.CreateNoWindow = true;

                files_to_process.Remove(filename);

                Process process = new Process();                
                process.StartInfo = processStartInfo;
                
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(ProcessExited);
                process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
                try
                {

                    String dst = proFileDir + "\\" + proFileName + ".vcxproj";
                    if (File.Exists(dst))
                    {
                        File.Delete(dst);
                    }

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.StandardInput.WriteLine(env);
                    process.StandardInput.WriteLine(cmd);
                    process.StandardInput.WriteLine("exit");

                    process.StandardInput.AutoFlush = true;

                }
                catch(Exception e)
                {
                    MessageBox.Show("Error: Could not canvert pro file " + e.ToString());
                }
            }
        }
        private void ProcessExited(object sender, EventArgs e)
        {
            files_remain--;
            if (files_remain > 0)
            {
                handle_pro();
            }
        }

        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    textBox1.AppendText(e.Data.TrimEnd('\r', '\n') + "\r\n");
                }
            }));
        }

        void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    textBox1.AppendText(e.Data.TrimEnd('\r', '\n') + "\r\n");
                }
            }));
        }

        private void pro2vcxj_Load(object sender, EventArgs e)
        {
            textBox2.Text = Properties.Settings.Default.vcDevCmd;
            textBox3.Text = Properties.Settings.Default.qmake;
        }
    }

}
