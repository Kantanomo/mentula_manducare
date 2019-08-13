﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using mentula_manducare.Classes;
using mentula_manducare.Objects;
using MentulaManducare;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace mentula_manducare.App_Code
{
    [HubName("ServerHub")]
    public class ServerHub : Hub
    {
        public dynamic CurrentUser =>
            Clients.Client(Context.ConnectionId);

        public string CurrentToken =>
            Context.QueryString["connectionToken"];
        public void LoginEvent(string Password)
        {
            try
            {
                var result = WebSocketThread.Users.Login(Password, CurrentToken);
                if (result.Result)
                {

                    Logger.AppendToLog("WebLogin",
                        $"{result.UserObject.Username}:{Context.Request.GetRemoteIpAddress()} has logged in");
                    CurrentUser.LoginEvent("Success", result.UserObject.Token);
                    MainThread.WriteLine($"WebUI Event: {result.UserObject.Username} has logged in");
                }
                else
                {
                    Logger.AppendToLog("WebLogin",
                        $"{Context.Request.GetRemoteIpAddress()} Invalid Login attempt");
                    CurrentUser.LoginEvent("Failure", "");
                }
            }
            catch(Exception)
            {
                MainThread.WriteLine("Unknown error has occured in WebSocket Server.... Restarting", true);
            }
        }
        public void GetServerListEvent()
        {
            if (!WebSocketThread.Users.TokenLogin(CurrentToken).Result) return;
            var a = new List<Dictionary<string, string>>();
            foreach (ServerContainer server in ServerThread.Servers)
            {
                if (server.Name != "") //Server is not in a state to send to client skip this pass.
                {
                    var b = new Dictionary<string, string>();
                    b.Add("Index", server.WebGuid.ToString());
                    b.Add("Name", server.Name);
                    b.Add("Instance", server.Instance);
                    a.Add(b);
                }
            }
            CurrentUser.GetServerListEvent(JsonConvert.SerializeObject(a));
        }
        public void GetCurrentPlayersEvent(string serverIndex)
        {
            if (!WebSocketThread.Users.TokenLogin(CurrentToken).Result) return;
            var server = ServerThread.Servers[Guid.Parse(serverIndex)];
            if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                var a = server.CurrentPlayers;
                var b = new List<Dictionary<string, string>>();
                foreach (PlayerContainer playerContainer in a)
                {
                    var c = new Dictionary<string, string>();
                    c.Add("Name", playerContainer.Name);
                    c.Add("Team", playerContainer.Team.ToString());
                    c.Add("Biped", playerContainer.Biped.ToString());
                    c.Add("EmblemURL", playerContainer.EmblemURL);
                    b.Add(c);
                }

                CurrentUser.GetPlayersListEvent(JsonConvert.SerializeObject(b));
            }
        }

        public void KickPlayerEvent(string serverIndex, string PlayerName)
        {
            var TokenResult = WebSocketThread.Users.TokenLogin(CurrentToken);
            if (!TokenResult.Result) return;
            var server = ServerThread.Servers[Guid.Parse(serverIndex)];
            if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                Logger.AppendToLog(server.LogName, $"{TokenResult.UserObject.Username} kicked player {PlayerName}");
                server.ConsoleProxy.KickPlayer(PlayerName);
                CurrentUser.KickPlayerEvent("Success");
            }
        }

        public void GetServerStatusEvent(string serverIndex)
        {
            if (!WebSocketThread.Users.TokenLogin(CurrentToken).Result) return;
            var a = new Dictionary<string, string>();
            var server = ServerThread.Servers[Guid.Parse(serverIndex)];
            if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                //The server will not update strings unless you status/skip
                server.ConsoleProxy.Status();
                a.Add("GameState", server.GameState.ToString());
                a.Add("CurrentMap", server.CurrentVariantMap);
                a.Add("CurrentName", server.CurrentVariantName);
                a.Add("NextMap", server.NextVariantMap);
                a.Add("NextName", server.NextVariantName);
                a.Add("ServerCount", ServerThread.Servers.Count.ToString()); //Hacked to pieces.
                CurrentUser.GetServerStatusEvent(JsonConvert.SerializeObject(a));
            }
        }

        public void SkipServerEvent(string serverIndex)
        {
            var TokenResult = WebSocketThread.Users.TokenLogin(CurrentToken);
            if (!TokenResult.Result) return;
            var server = ServerThread.Servers[Guid.Parse(serverIndex)];
            if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                Logger.AppendToLog(server.LogName, $"{TokenResult.UserObject.Username} skipped match");
                server.ConsoleProxy.Skip();
                CurrentUser.SkipServerEvent("Success");
            }
        }

        public void LoadBanListEvent(string serverIndex)
        {
            if (!WebSocketThread.Users.TokenLogin(CurrentToken).Result) return;
            CurrentUser.LoadBanListEvent(
                JsonConvert.SerializeObject(
                    ServerThread.Servers[Guid.Parse(serverIndex)].GetBannedGamers)
                );
        }

        public void BanPlayerEvent(string serverIndex, string playerName)
        {
            var TokenResult = WebSocketThread.Users.TokenLogin(CurrentToken);
            if (!TokenResult.Result) return;
            var server = ServerThread.Servers[Guid.Parse(serverIndex)]; if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                Logger.AppendToLog(server.LogName, $"{TokenResult.UserObject.Username} banned player {playerName}");
                server.ConsoleProxy.BanPlayer(playerName);
                CurrentUser.BanPlayerEvent("Success");
            }
        }

        public void UnBanPlayerEvent(string serverIndex, string playerName)
        {
            var TokenResult = WebSocketThread.Users.TokenLogin(CurrentToken);
            if (!TokenResult.Result) return;
            var server = ServerThread.Servers[Guid.Parse(serverIndex)];
            if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                Logger.AppendToLog(server.LogName, $"{TokenResult.UserObject.Username} unbanned player {playerName}");
                server.ConsoleProxy.UnBanPlayer(playerName);
                CurrentUser.UnBanPlayerEvent("Success");
            }
        }

        public void LoadPlaylistsEvent()
        {
            if (!WebSocketThread.Users.TokenLogin(CurrentToken).Result) return;
            var playlists = Directory.GetFiles(ServerThread.PlaylistFolder);
            List<string> nlists = new List<string>();
            foreach (var playlist in playlists)
                nlists.Add(playlist.Replace(ServerThread.PlaylistFolder, ""));       
            CurrentUser.LoadPlaylistsEvent(JsonConvert.SerializeObject(nlists));
        }

        public void ChangePlaylistEvent(string serverIndex, string playlistname)
        {
            var TokenResult = WebSocketThread.Users.TokenLogin(CurrentToken);
            if (!TokenResult.Result) return;
            var server = ServerThread.Servers[Guid.Parse(serverIndex)];
            if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                Logger.AppendToLog(server.LogName,
                    $"{TokenResult.UserObject.Username} changed playlist to {playlistname}");
                ServerThread.Servers[Guid.Parse(serverIndex)].ConsoleProxy.SetPlaylist(playlistname);
                CurrentUser.ChangePlaylistEvent("Success");
            }
        }

        public void LoadServerLogEvent(string serverIndex)
        {
            var TokenResult = WebSocketThread.Users.TokenLogin(CurrentToken);
            if (!TokenResult.Result) return;
            var server = ServerThread.Servers[Guid.Parse(serverIndex)];
            if (server == null)
            {
                NotifyServerChangeEvent();
            }
            else
            {
                var l = JsonConvert.SerializeObject(Logger.GetLog(server.LogName).DumpLogs());
                CurrentUser.LoadServerLogEvent(l);
            }
        }

        public void NotifyServerChangeEvent()
        {
            Clients.All.NotifyServerChangeEvent();
        }
        public static void NotifyServerChangeEventEx()
        {
            GlobalHost.ConnectionManager.GetHubContext<ServerHub>().Clients.All.NotifyServerChangeEvent();
        }
    }
}
