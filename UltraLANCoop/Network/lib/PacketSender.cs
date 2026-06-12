using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace UltraLANCoop.Network
{
    public static class PacketSender
    {
        public static void SendToAll(NetDataWriter writer, DeliveryMethod method = DeliveryMethod.ReliableOrdered)
        {
            if (!NetCore.IsConnected) return;

            foreach (var peer in ConnectionManager.Peers.Values)
                peer.Send(writer, method);
        }
        public static void SendRaw(NetDataWriter writer, DeliveryMethod method = DeliveryMethod.ReliableOrdered)
        {
            SendToAll(writer, method);
        }
        public static void SendDamage(int targetId, int damage, int attackerId)
        {
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.Damage);
            writer.Put(targetId);
            writer.Put(damage);
            writer.Put(attackerId);
            SendToAll(writer);
        }

        public static void SendDeath(int entityId, int killerId)
        {
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.Death);
            writer.Put(entityId);
            writer.Put(killerId);
            SendToAll(writer);
        }

        public static void SendWeaponFire(int shooterId, byte weaponType, byte shotType, byte gunVariation, Vector3 origin, Vector3 direction)
        {
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.WeaponFire);
            writer.Put(shooterId);
            writer.Put(weaponType);
            writer.Put(shotType);
            writer.Put(gunVariation);
            writer.Put(origin.x);
            writer.Put(origin.y);
            writer.Put(origin.z);
            writer.Put(direction.x);
            writer.Put(direction.y);
            writer.Put(direction.z);
            SendToAll(writer);
        }

        public static void SendEnemyDamage(int entityId, float multiplier, float critMultiplier)
        {
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.EnemyDamage);
            writer.Put(entityId);
            writer.Put(multiplier);
            writer.Put(critMultiplier);
            SendToAll(writer);
        }

        public static void SendEnemyDeath(int entityId, bool fromExplosion)
        {
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.EnemyDeath);
            writer.Put(entityId);
            writer.Put(fromExplosion);
            SendToAll(writer);
        }
    }
}