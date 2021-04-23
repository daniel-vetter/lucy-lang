using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Lucy.DebugUI.WebShell
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo();
            p.StartInfo.FileName = @"dotnet";
            p.StartInfo.Arguments = "run";
            p.StartInfo.WorkingDirectory = FindDebugUIProjectFolder();
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            var job = new Job();
            job.AddProcess(p.Id);

            webView.Source = new Uri("http://localhost:5000");
        }

        string FindDebugUIProjectFolder()
        {
            var dir = AppContext.BaseDirectory;
            var searchFor = "Lucy.DebugUI";
            while (true)
            {
                var testPath = Path.Combine(dir, searchFor);
                if (Directory.Exists(testPath))
                    return testPath;

                dir = Path.GetDirectoryName(dir);
            }
        }
    }
}
