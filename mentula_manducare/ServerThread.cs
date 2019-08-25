using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mentula_manducare.App_Code;
using mentula_manducare.Classes;
using mentula_manducare.Objects;
using MentulaManducare;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace mentula_manducare
{

    public static class ServerThread
    {


        public static string ExecutionPath { get; set; }

        public static string PlaylistFolder =>
            $"{ExecutionPath}\\Playlists\\";

        public static ServerCollection Servers = new ServerCollection();
        public static int TickCount = 0;
        public static TimeSpan TPS = TimeSpan.Zero;
        public static void Run()
        {


            DetectServers(true);
            while (true)
            {
                var watch = Stopwatch.StartNew();

                foreach (ServerContainer serverContainer in Servers)
                    serverContainer.Tick();

                Thread.Sleep(15);
                watch.Stop();
                TPS = TPS.Add(watch.Elapsed);
                TickCount++;
                if (TPS.Seconds >= 1)
                {
                    //MainThread.WriteLine($"Current Server Thread Tickrate: {TickCount}");
                    //Going for 50-70 TPS
                    TickCount = 0;
                    TPS = TimeSpan.Zero;
                    DetectServers();
                }


            }
        }

        public static void DetectServers(bool Inital = false)
        {
            var serverProcesses = Process.GetProcessesByName("h2Server");
            if (Inital)
            {
                MainThread.WriteLine($"Servers Detected:\t{serverProcesses.Length}");
                if (serverProcesses.Length > 0)
                {
                    ExecutionPath = serverProcesses[0].MainModule?.FileName.Replace("\\h2server.exe", "")
                        .Replace("\\H2Server.exe", "");
                    MainThread.WriteLine($"Server Launch Path:\t{ExecutionPath}");
                    MainThread.WriteLine($"Server Playlist Folder:\t{PlaylistFolder}");
                }
            }
            else
            {
                for (var i = 0; i < Servers.Count; i++)
                {
                    if (Servers[i].ServerProcess.HasExited)
                    {
                        if (!Servers[i].AutoRestart)
                        {
                            MainThread.WriteLine($"{Servers[i].FormattedName} has closed detaching..");
                            Servers[i].KillConsoleProxy();
                            Servers.RemoveAt(i);
                        }
                        else
                        {
                            MainThread.WriteLine($"{Servers[i].FormattedName} has closed restarting service..");
                            var Service = new ServiceController(Servers[i].ServiceName);
                            if (Service.Status != ServiceControllerStatus.Stopped)
                            {
                                Service.Stop();
                                Servers[i].ServerProcess.Kill();
                            }
                            Service.Start();
                            Servers[i].KillConsoleProxy();
                            Servers.RemoveAt(i);
                        }
                    }

                }
            }
            for (var i = 0; i < serverProcesses.Length; i++)
            {
                if (!Servers.ServerCollected(serverProcesses[i]))
                {
                    MainThread.WriteLine($"Attaching To Server...");
                    var newServer = new ServerContainer(serverProcesses[i]);
                    newServer.Index = i;
                    if (newServer.isLive)
                    {
                        MainThread.WriteLine($"Attached to: {newServer.FormattedName}");
                        MainThread.WriteLine($"Service Name: {newServer.ServiceName}");
                        newServer.LaunchConsoleProxy();
                        Servers.Add(newServer);
                    }
                    else
                        MainThread.WriteLine(
                            $"Skipping LAN Server: {newServer.FormattedName}");
                }
            }
        }
    }
}
