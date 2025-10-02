using System ;
using System.IO ;
using System.Linq;
using System.Reactive.Linq;
using System.Threading ;
using System.Threading.Tasks ;
using Autofac ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Idasen.Launcher ;
using Microsoft.Extensions.Configuration ;
using Serilog ;
using static System.Console ;

namespace Idasen.ConsoleApp
{
    internal sealed class Program
    {
        /// <summary>
        ///     Test Application
        /// </summary>
        private static async Task Main (string[] args)
        {
            var tokenSource = new CancellationTokenSource ( TimeSpan.FromSeconds ( 60 ) ) ;
            var token       = tokenSource.Token ;

            var builder = new ConfigurationBuilder ( ).SetBasePath ( Directory.GetCurrentDirectory ( ) )
                                                      .AddJsonFile ( "appsettings.json" ) ;

            var container = ContainerProvider.Create ( builder.Build ( ) ) ;

            var logger   = container.Resolve < ILogger > ( ) ;
            var provider = container.Resolve < IDeskProvider > ( ) ;

            provider.Initialize ( DefaultDeviceName ,
                                  DefaultDeviceAddress ,
                                  DefaultDeviceMonitoringTimeout ) ;

            var (isSuccess , desk) = await provider.TryGetDesk ( token ) ;

            if (args.Length == 1)
            {
                var height = uint.Parse(args[0]);
                if (height < 6300u)
                {
                    height = 6600u;
                }
                if (height > 11800u)
                {
                    height = 11800u;
                }
                if (isSuccess)
                    desk!.MoveTo(height);
                else
                    logger.Error("Failed to detect desk");

                desk.FinishedChanged.Subscribe(p => System.Environment.Exit(0));
            }

            desk.MoveLock();
            //
            Console.ReadKey();

        }

        private const string DefaultDeviceName              = "Desk 2295" ;
        private const ulong  DefaultDeviceAddress           = 235696085912647u ;
        private const uint   DefaultDeviceMonitoringTimeout = 600u ;
    }
}