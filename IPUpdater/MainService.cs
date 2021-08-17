using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;

namespace IPUpdater
{
    public class MainService
    {
        private string _externalIP = string.Empty;
        private Timer _timer;

        public MainService()
        {
        }

        public void Run()
        {
            _timer = new Timer(state =>
                {
                    try
                    {
                        string externalip = new WebClient().DownloadString("http://icanhazip.com").Replace("\n", "");
                        Program.Log.Info("Downloaded IP: " + externalip);
                        Program.Log.Info("Current IP: " + _externalIP);

                        if (_externalIP != externalip)
                        {
                            Program.Log.Info("Changing IP");
                            var res = new WebClient().DownloadString($"http://mosalski.de/ip.php?hostname=a.mosalski.de&myip={externalip}&guid=6c37a4b2-9f0b-497f-9be8-7e9d3923c828");

                            Program.Log.Info("Result: " + res);
                            if (res == "good" || res.Contains("nochg"))
                            {
                                Program.Log.Info("Ip changed");
                                _externalIP = externalip;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Program.Log.Error("Error", e);
                    }
                }, null, 0, 5 * 1000 * 60);
        }

        private string FullException(Exception exception)
        {
            var res = string.Empty;

            if (exception != null)
            {
                res += exception.Message + Environment.NewLine;
                res += exception.StackTrace + Environment.NewLine;
                res += FullException(exception.InnerException);
            }

            return res;
        }

        public void Stop()
        {
            _timer?.Dispose();
        }
    }
}
