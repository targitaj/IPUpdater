using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace IPUpdater
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {


            };

            HostFactory.Run((x) =>
            {
                x.Service<MainService>(s =>
                {
                    s.ConstructUsing(name => new MainService());
                    s.WhenStarted(tc => tc.Run());
                    s.WhenStopped(tc => tc.Stop());


                });

                x.SetDescription("IPUpdater");
                x.SetDisplayName("IPUpdater");
                x.SetServiceName("IPUpdater");

                x.StartAutomatically();
            });
        }
    }
}
