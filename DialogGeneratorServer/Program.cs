using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogGeneratorServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Logger.LogLevel = Logger.Level.Debug;
#endif
            Logger.LogDebug("new Server()");
            var server = new Server();
            Logger.Log("Server start");
            server.Start();

            Console.WriteLine("start");
            Console.ReadKey(true);

            Logger.Log("Server stop");
            server.Stop();
            Logger.LogDebug("server.Dispose()");
            server.Dispose();
        }
    }
}
