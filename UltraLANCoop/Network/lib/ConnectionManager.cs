using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using BepInEx.Logging;
using UltraLANCoop.Network.Entities;

namespace UltraLANCoop.Network
{
    // Управление сетевыми подключениями: сервер, клиент, peer-события
    public static class ConnectionManager
    {
        private static NetManager _netManager;
        private static EventBasedNetListener _listener;
        private static readonly Dictionary<int, NetPeer> _peers = new Dictionary<int, NetPeer>();

        private static ManualLogSource Log => Plugin.Instance.Logger;

        // События
        public static event Action<int> OnPeerConnected;
        public static event Action<int, DisconnectInfo> OnPeerDisconnected;
        public static event Action<NetPeer, NetPacketReader, byte, DeliveryMethod> OnNetworkReceive;

        // Свойства
        public static bool IsServer => _netManager != null && _netManager.IsRunning && NetCore.IsServer;
        public static bool IsClient => _netManager != null && _netManager.IsRunning && NetCore.IsClient;
        public static IReadOnlyDictionary<int, NetPeer> Peers => _peers;

        public static void StartServer(int port)
        {
            Stop();
            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener) { AutoRecycle = true, UpdateTime = 15 };

            _listener.ConnectionRequestEvent += req => req.AcceptIfKey("UltraLANCoop");
            _listener.PeerConnectedEvent += HandlePeerConnected;
            _listener.PeerDisconnectedEvent += HandlePeerDisconnected;
            _listener.NetworkReceiveEvent += HandleNetworkReceive;
            _listener.NetworkErrorEvent += (ep, err) => Log.LogError("[Connection] Error: " + err);
            _listener.NetworkLatencyUpdateEvent += (peer, latency) =>
            {
                if (PlayerManager.TryGetPlayer(peer.Id, out var p))
                    p.Ping = latency;
            };

            try
            {
                _netManager.Start(port);
                Log.LogInfo("[Connection] Server started on port " + port);
            }
            catch (Exception e)
            {
                Log.LogError("[Connection] Failed to start server: " + e.Message);
                Stop();
                throw;
            }
        }

        // Подключение к серверу по IP и порту
        public static void ConnectToServer(string ip, int port)
        {
            Stop();
            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener) { AutoRecycle = true, UpdateTime = 15 };

            _listener.PeerConnectedEvent += HandlePeerConnected;
            _listener.PeerDisconnectedEvent += HandlePeerDisconnected;
            _listener.NetworkReceiveEvent += HandleNetworkReceive;
            _listener.NetworkErrorEvent += (ep, err) => Log.LogError("[Connection] Error: " + err);
            _listener.NetworkLatencyUpdateEvent += (peer, latency) =>
            {
                if (PlayerManager.TryGetPlayer(peer.Id, out var p))
                    p.Ping = latency;
            };

            try
            {
                _netManager.Start();
                _netManager.Connect(ip, port, "UltraLANCoop");
                Log.LogInfo("[Connection] Connecting to " + ip + ":" + port + "...");
            }
            catch (Exception e)
            {
                Log.LogError("[Connection] Failed to connect: " + e.Message);
                Stop();
                throw;
            }
        }
        // Остановка всех сетевых соединений
        public static void Stop()
        {
            _netManager?.Stop();
            _netManager = null;
            _peers.Clear();
        }
        // Опрос событий LiteNetLib. Должен вызываться каждый кадр
        public static void PollEvents()
        {
            _netManager?.PollEvents();
        }
        // Отключение конкретного игрока (только для сервера)
        public static void KickPeer(int playerId)
        {
            if (_peers.TryGetValue(playerId, out var peer))
            {
                peer.Disconnect();
                _peers.Remove(playerId);
                Log.LogInfo("[Connection] Kicked peer " + playerId);
            }
        }
        private static void HandlePeerConnected(NetPeer peer)
        {
            _peers[peer.Id] = peer;
            OnPeerConnected?.Invoke(peer.Id);
        }

        private static void HandlePeerDisconnected(NetPeer peer, DisconnectInfo info)
        {
            _peers.Remove(peer.Id);
            OnPeerDisconnected?.Invoke(peer.Id, info);
        }

        private static void HandleNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            OnNetworkReceive?.Invoke(peer, reader, channelNumber, deliveryMethod);
        }
    }
}