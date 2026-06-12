using UnityEngine;
using BepInEx.Logging;
using UltraLANCoop.Network;

namespace UltraLANCoop.UI
{
    public class MainMenu : UiWindow
    {
        private static ManualLogSource Log => Plugin.Instance.Logger;

        private string playerName = "Player";
        private string serverIp = "127.0.0.1";
        private int port = 7777;
        private Vector2 playersScroll = Vector2.zero;

        protected override void Awake()
        {
            base.Awake();
            windowRect = new Rect(20, 20, 400, 350);
            Log.LogInfo("[MainMenu] Awake called");
            
            playerName = PlayerPrefs.GetString("ultraLanCoop_playerName", "Player");
            NetCore.LocalPlayerName = playerName;
        }
        
        protected override int GetWindowId() => 12345;
        
        protected override void DrawWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            
            GUILayout.Label("Your Name:", GUILayout.Width(200));
            string newName = GUILayout.TextField(playerName, GUILayout.ExpandWidth(true), GUILayout.Height(22));
            if (newName != playerName)
            {
                playerName = newName;
                NetCore.LocalPlayerName = playerName;
                PlayerPrefs.SetString("ultraLanCoop_playerName", playerName);
                PlayerPrefs.Save();
            }
            
            GUILayout.Space(10);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:", GUILayout.Width(25));
            serverIp = GUILayout.TextField(serverIp, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);
            GUILayout.Label("Port:", GUILayout.Width(35));
            string portStr = GUILayout.TextField(port.ToString(), GUILayout.Width(60));
            if (int.TryParse(portStr, out int newPort) && newPort > 0 && newPort < 65536)
            {
                port = newPort;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUI.enabled = !NetCore.IsConnected;
            if (GUILayout.Button("Start Server", GUILayout.Height(30)))
            {
                Log.LogInfo($"[MainMenu] Start Server clicked! Name: {playerName}");
                NetCore.StartServer(port);
            }
            if (GUILayout.Button("Connect", GUILayout.Height(30)))
            {
                Log.LogInfo($"[MainMenu] Connect clicked! Name: {playerName} to {serverIp}:{port}");
                NetCore.ConnectToServer(serverIp, port);
            }
            GUI.enabled = NetCore.IsConnected;
            if (GUILayout.Button("Disconnect", GUILayout.Height(30)))
            {
                NetCore.Stop();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);
            GUILayout.Label("--- Players ---");
            
            if (!NetCore.IsConnected)
            {
                GUILayout.Label("Not connected");
            }
            else
            {
                var players = NetCore.GetAllPlayers();
                if (players.Length == 0)
                {
                    GUILayout.Label("No players");
                }
                else
                {
                    playersScroll = GUILayout.BeginScrollView(playersScroll, GUILayout.Height(150));
                    foreach (var p in players)
                    {
                        GUILayout.BeginHorizontal();
                        string name = p.Name;
                        if (p.IsLocal) name += " (You)";
                        if (p.IsHost) name += " [Host]";
                        GUILayout.Label(name, GUILayout.Width(150));
                        
                        Color oldColor = GUI.color;
                        float hpPercent = p.MaxHP > 0 ? (float)p.HP / p.MaxHP : 0f;
                        GUI.color = hpPercent > 0.6f ? Color.green : (hpPercent > 0.3f ? Color.yellow : Color.red);
                        GUILayout.Label($"{p.HP} / {p.MaxHP}", GUILayout.Width(70));
                        
                        GUI.color = p.Ping < 50 ? Color.green : (p.Ping < 100 ? Color.yellow : Color.red);
                        GUILayout.Label(p.Ping + "ms", GUILayout.Width(50));
                        GUI.color = oldColor;
                        
                        if (p.IsHost || p.IsLocal)
                        {
                            GUILayout.Label("  ", GUILayout.Width(60));
                        }
                        else
                        {
                            GUI.enabled = NetCore.IsServer;
                            if (GUILayout.Button("Kick", GUILayout.Width(60), GUILayout.Height(20))) NetCore.KickPlayer(p.Id);
                            GUI.enabled = true;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}