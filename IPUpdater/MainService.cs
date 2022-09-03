using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Newtonsoft.Json;

namespace IPUpdater
{
    public class MainService
    {
        private string _externalIP = string.Empty;
        private Timer _timer;

        public MainService()
        {
			//aaa
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
                            var mydomain = "mosalski.de";
                            var myhostname = "a";
                            var gdapikey = "9QCBbJDW9fJ_VW7EEyN9V9mKB8QxT4Vgtk:BXf8PgDpZDqZ9NnuFqd83F";
                            var logdest = "local7.info";

                            var res = RunCurl($@"curl -X GET -H ""Authorization: sso-key {gdapikey}"" ""https://api.godaddy.com/v1/domains/{mydomain}/records/A/{myhostname}""");
                            var ipData = JsonConvert.DeserializeObject<IPData[]>(res);

                            if (ipData[0].data == externalip)
                            {
                                _externalIP = ipData[0].data;
                            }
                            else
                            {
                                var command =
                                    $@"curl -X PUT ""https://api.godaddy.com/v1/domains/{mydomain}/records/A/{myhostname}"" -H ""Authorization: sso-key {gdapikey}"" -H ""Content-Type: application/json"" -d ""[{{\""data\"": \""{externalip}\""}}]""";

                                res = RunCurl(command);

                                Program.Log.Info("Result: " + res);
                                if (string.IsNullOrWhiteSpace(res))
                                {
                                    Program.Log.Info("Ip changed");
                                    _externalIP = externalip;
                                }
                            }


                        }
                    }
                    catch (Exception e)
                    {
                        Program.Log.Error("Error", e);
                    }
                }, null, 0, 5 * 1000 * 60);
        }

        public string RunCurl(string command)
        {
            using (var proc = new Process
                   {
                       StartInfo = new ProcessStartInfo
                       {
                           FileName = "curl.exe",
                           Arguments = command,
                           UseShellExecute = false,
                           RedirectStandardOutput = true,
                           RedirectStandardError = true,
                           CreateNoWindow = true,
                           WorkingDirectory = Environment.SystemDirectory
                       }
                   })
            {
                proc.Start();

                proc.WaitForExit(5000);

                return proc.StandardOutput.ReadToEnd();
            }
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
