using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using UltraLANCoop.Network.Entities;
using BepInEx.Logging;

namespace UltraLANCoop.Network
{
    public static class PacketHandler
    {
        private static ManualLogSource Log => Plugin.Instance.Logger;
        public static void HandlePacket(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (reader.AvailableBytes < 1) return;

            var type = (PacketType)reader.GetByte();

            switch (type)
            {
                case PacketType.Snapshot:
                    EntityPool.HandleSnapshot(reader);
                    break;
                case PacketType.Damage:
                    HandleDamage(reader);
                    break;
                case PacketType.Death:
                    HandleDeath(reader);
                    break;
                case PacketType.WeaponSwitch:
                    HandleWeaponSwitch(reader);
                    break;
                case PacketType.WeaponFire:
                    HandleWeaponFire(reader);
                    break;
                case PacketType.EnemyDamage:
                    HandleEnemyDamage(reader);
                    break;
                case PacketType.EnemyDeath:
                    HandleEnemyDeath(reader);
                    break;
                default:
                    var data = reader.GetRemainingBytes();
                    NetCore.RaisePacketReceived(type, data, peer.Id);
                    break;
            }
        }
        private static void HandleDamage(NetPacketReader reader)
        {
            int targetId = reader.GetInt();
            int damage = reader.GetInt();
            int attackerId = reader.GetInt();

            Log.LogInfo($"[PacketHandler] Получен урон: цель={targetId}, урон={damage}");

            EntityPool.Get(targetId)?.ApplyDamage(damage, attackerId);

            if (PlayerManager.TryGetPlayer(targetId, out var playerData))
            {
                playerData.HP -= damage;
                if (playerData.HP < 0) playerData.HP = 0;
            }
        }

        private static void HandleDeath(NetPacketReader reader)
        {
            int entityId = reader.GetInt();
            int killerId = reader.GetInt();

            Log.LogInfo($"[PacketHandler] Получена смерть: сущность={entityId}");

            EntityPool.Get(entityId)?.Kill(killerId);

            if (PlayerManager.TryGetPlayer(entityId, out var playerData))
                playerData.HP = 0;
        }

        private static void HandleWeaponSwitch(NetPacketReader reader)
        {
            int playerId = reader.GetInt();
            int slotIndex = reader.GetInt();
            string weaponName = reader.GetString();

            Debug.Log($"[PacketHandler] Player {playerId} switched to: {weaponName} (slot {slotIndex})");
        }

        private static void HandleWeaponFire(NetPacketReader reader)
        {
            int playerId = reader.GetInt();
            if (playerId == NetCore.LocalPlayerId) return;

            byte weaponType = reader.GetByte();
            byte shotType = reader.GetByte();
            byte gunVariation = reader.GetByte();

            Vector3 position = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            Vector3 direction = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());

            Log.LogInfo($"[PacketHandler] WeaponFire from {playerId}: weapon={weaponType}, shot={shotType}");

            VisualEffects.PlayWeaponFireEffect(playerId, weaponType, shotType, gunVariation, position, direction);
        }

        private static void HandleEnemyDamage(NetPacketReader reader)
        {
            int entityId = reader.GetInt();
            float multiplier = reader.GetFloat();
            float critMultiplier = reader.GetFloat();

            Log.LogInfo($"[PacketHandler] Enemy damage: entity={entityId}, mult={multiplier}");

            if (EntityPool.Get(entityId) is EnemyEntity enemy)
                enemy.ApplyDamage((int)multiplier, -1);
        }

        private static void HandleEnemyDeath(NetPacketReader reader)
        {
            int entityId = reader.GetInt();
            bool fromExplosion = reader.GetBool();

            Log.LogInfo($"[PacketHandler] Enemy death: entity={entityId}");

            if (EntityPool.Get(entityId) is EnemyEntity enemy)
                enemy.Kill(-1);
        }
    }
}