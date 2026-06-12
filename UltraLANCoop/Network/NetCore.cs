using System;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using UltraLANCoop.Network.Entities;
using BepInEx.Logging;

namespace UltraLANCoop.Network
{
    public static class NetCore
    {
        public const int TICKS_PER_SECOND = 20;
        public static bool IsServer { get; private set; }
        public static bool IsClient { get; private set; }
        public static bool IsConnected => IsServer || IsClient;

        public static int LocalPlayerId { get; private set; }
        public static string LocalPlayerName { get; set; } = "Player";

        private static float _lastSnapshotTime = 0f;

        public static event Action<int> OnPlayerJoined;
        public static event Action<int> OnPlayerLeft;
        public static event Action<PacketType, byte[], int> OnPacketReceived;
        public static event Action OnServerStarted;
        public static event Action OnClientConnected;
        public static event Action OnDisconnected;

        private static ManualLogSource Log => Plugin.Instance.Logger;

        public static void RaisePacketReceived(PacketType type, byte[] data, int peerId) => OnPacketReceived?.Invoke(type, data, peerId);

        public static void Load()
        {
            Log.LogInfo("[NetCore] Initializing...");
            ConnectionManager.OnPeerConnected += HandlePeerConnected;
            ConnectionManager.OnPeerDisconnected += HandlePeerDisconnected;
            ConnectionManager.OnNetworkReceive += PacketHandler.HandlePacket;
        }

        public static void Unload()
        {
            Stop();
            Log.LogInfo("[NetCore] Unloaded");
        }

        public static void Update()
        {
            ConnectionManager.PollEvents();

            if (IsConnected && Time.time - _lastSnapshotTime >= 1f / TICKS_PER_SECOND)
            {
                EntityPool.SendSnapshots();
                _lastSnapshotTime = Time.time;
            }

            if (IsConnected)
            {
                EntityPool.UpdateAll(Time.deltaTime);
            }
        }

        public static void StartServer(int port)
        {
            ResetState();
            IsServer = true;
            LocalPlayerId = 0;

            PlayerManager.AddPlayer(0, LocalPlayerName, isLocal: true, isHost: true);
            var localPlayer = new PlayerEntity(0, 0);
            EntityPool.Add(localPlayer);

            try
            {
                ConnectionManager.StartServer(port);
                OnServerStarted?.Invoke();
            }
            catch (Exception e)
            {
                Log.LogError($"[NetCore] Failed to start server: {e.Message}");
                Stop();
            }
        }

        public static void ConnectToServer(string ip, int port)
        {
            ResetState();
            IsClient = true;

            try
            {
                ConnectionManager.ConnectToServer(ip, port);
                OnClientConnected?.Invoke();
            }
            catch (Exception e)
            {
                Log.LogError($"[NetCore] Failed to connect: {e.Message}");
                Stop();
            }
        }

        public static void Stop()
        {
            EntityPool.Clear();
            PlayerManager.Clear();
            ConnectionManager.Stop();
            IsServer = false;
            IsClient = false;
            OnDisconnected?.Invoke();
        }

        public static void SendToAll(NetDataWriter writer, DeliveryMethod method) => PacketSender.SendToAll(writer, method);
        public static void SendRaw(NetDataWriter writer, DeliveryMethod method = DeliveryMethod.ReliableOrdered) => PacketSender.SendRaw(writer, method);
        public static void SendDamage(int targetId, int damage, int attackerId) => PacketSender.SendDamage(targetId, damage, attackerId);
        public static void SendDeath(int entityId, int killerId) => PacketSender.SendDeath(entityId, killerId);
        public static void SendWeaponFire(int shooterId, byte weaponType, byte shotType, byte gunVariation, Vector3 origin, Vector3 direction) => PacketSender.SendWeaponFire(shooterId, weaponType, shotType, gunVariation, origin, direction);
        public static void SendEnemyDamage(int entityId, float multiplier, float critMultiplier) => PacketSender.SendEnemyDamage(entityId, multiplier, critMultiplier);
        public static void SendEnemyDeath(int entityId, bool fromExplosion) => PacketSender.SendEnemyDeath(entityId, fromExplosion);
        public static PlayerData[] GetAllPlayers() => PlayerManager.GetAllPlayers();
        public static void UpdatePlayer(int playerId, int hp, int maxHp) => PlayerManager.UpdatePlayerHP(playerId, hp, maxHp);
        public static int GetMaxHP() => PlayerManager.GetMaxHP();
        public static void KickPlayer(int playerId) => ConnectionManager.KickPeer(playerId);

        private static void ResetState()
        {
            EntityPool.Clear();
            PlayerManager.Clear();
            ConnectionManager.Stop();
            IsServer = false;
            IsClient = false;
        }

        private static void HandlePeerConnected(int peerId)
        {
            if (IsServer)
            {
                PlayerManager.AddPlayer(peerId, "Player " + peerId, isLocal: false, isHost: false);
                var remotePlayer = new PlayerEntity(peerId, peerId);
                EntityPool.Add(remotePlayer);
                
                Log.LogInfo($"[NetCore] Player {peerId} connected and entity created");
                OnPlayerJoined?.Invoke(peerId);
            }
            else if (IsClient)
            {
                LocalPlayerId = 1;
                PlayerManager.AddPlayer(LocalPlayerId, LocalPlayerName, isLocal: true, isHost: false);

                var localPlayer = new PlayerEntity(LocalPlayerId, LocalPlayerId);
                EntityPool.Add(localPlayer);

                Log.LogInfo($"[NetCore] Connected to server, my LocalPlayerId is: {LocalPlayerId}");
            }
        }

        private static void HandlePeerDisconnected(int peerId, DisconnectInfo info)
        {
            Log.LogInfo($"[NetCore] Player {peerId} disconnected. Reason: {info.Reason}");
            PlayerManager.RemovePlayer(peerId);
            EntityPool.Remove(peerId);
            OnPlayerLeft?.Invoke(peerId);
        }
    }
}