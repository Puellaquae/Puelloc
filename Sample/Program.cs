using System;
using System.Reflection.Metadata.Ecma335;
using Puelloc;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Setting setting = new Setting();
            setting.BindIP.Port = 0;
            Pipe hello = new Pipe("GET", "/", _ => new ResponseMessage("Hello"));
            HttpClient httpClient = new HttpClient(setting, hello);
            httpClient.Listen();
            if (!httpClient.IsRunning)
            {
                Console.WriteLine("Listen Fail.");
                return;
            }
            Console.WriteLine(httpClient.LocalEndPoint.ToString());
            string cmd = Console.ReadLine();
            while (cmd != "exit")
            {
                if (cmd == "add")
                {
                    httpClient.AddRoutes(new Pipe("GET","/add",_=>new ResponseMessage("This page adds during running")));
                }
                cmd = Console.ReadLine();
            }
            httpClient.Stop();
        }
    }
}
