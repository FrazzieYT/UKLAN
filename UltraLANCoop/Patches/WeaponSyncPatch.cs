using HarmonyLib;
using UnityEngine;
using UltraLANCoop.Network;
using UltraLANCoop.Harmony;
using LiteNetLib.Utils;

namespace UltraLANCoop.Patches
{
    public static class WeaponSyncPatch
    {
        private static void BuildAndSendWeaponFire(byte weaponType, byte shotType, byte variation)
        {
            if (!NetCore.IsConnected || CameraController.Instance?.transform == null) return;

            var cam = CameraController.Instance.transform;
            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.WeaponFire);
            writer.Put(NetCore.LocalPlayerId);
            writer.Put(weaponType);
            writer.Put(shotType);
            writer.Put(variation);
            writer.Put(cam.position.x);
            writer.Put(cam.position.y);
            writer.Put(cam.position.z);
            writer.Put(cam.forward.x);
            writer.Put(cam.forward.y);
            writer.Put(cam.forward.z);

            NetCore.SendToAll(writer, LiteNetLib.DeliveryMethod.ReliableOrdered);
        }

        [DynamicPatch(typeof(GunControl), "SwitchWeapon",
            typeof(int), typeof(int?), typeof(bool), typeof(bool), typeof(bool))]
        [Postfix]
        public static void OnSwitchWeapon(GunControl __instance, int targetSlotIndex)
        {
            if (!NetCore.IsConnected || __instance.currentWeapon == null) return;

            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.WeaponSwitch);
            writer.Put(NetCore.LocalPlayerId);
            writer.Put(targetSlotIndex);
            writer.Put(__instance.currentWeapon.name);

            NetCore.SendToAll(writer, LiteNetLib.DeliveryMethod.ReliableOrdered);
        }

        [DynamicPatch(typeof(Revolver), "Shoot", typeof(int))]
        [Postfix]
        public static void OnRevolverShoot(Revolver __instance, int shotType)
            => BuildAndSendWeaponFire(0, (byte)shotType, (byte)__instance.gunVariation);

        [DynamicPatch(typeof(Shotgun), "Shoot")]
        [Postfix]
        public static void OnShotgunShoot(Shotgun __instance)
            => BuildAndSendWeaponFire(1, 0, (byte)__instance.variation);

        [DynamicPatch(typeof(Shotgun), "ShootSinks")]
        [Postfix]
        public static void OnShotgunShootSinks(Shotgun __instance)
            => BuildAndSendWeaponFire(1, 1, (byte)__instance.variation);

        [DynamicPatch(typeof(Shotgun), "ShootSaw", typeof(bool))]
        [Postfix]
        public static void OnShotgunShootSaw(Shotgun __instance, bool noSaw)
            => BuildAndSendWeaponFire(1, 2, (byte)__instance.variation);

        [DynamicPatch(typeof(Shotgun), "Pump")]
        [Postfix]
        public static void OnShotgunPump(Shotgun __instance)
            => BuildAndSendWeaponFire(1, 3, (byte)__instance.variation);

        [DynamicPatch(typeof(Nailgun), "Shoot")]
        [Postfix]
        public static void OnNailgunShoot(Nailgun __instance)
            => BuildAndSendWeaponFire(2, 0, (byte)__instance.variation);

        [DynamicPatch(typeof(Nailgun), "BurstFire")]
        [Postfix]
        public static void OnNailgunBurstFire(Nailgun __instance)
            => BuildAndSendWeaponFire(2, 1, (byte)__instance.variation);

        [DynamicPatch(typeof(Nailgun), "SuperSaw")]
        [Postfix]
        public static void OnNailgunSuperSaw(Nailgun __instance)
            => BuildAndSendWeaponFire(2, 2, (byte)__instance.variation);

        [DynamicPatch(typeof(Nailgun), "ShootMagnet")]
        [Postfix]
        public static void OnNailgunShootMagnet(Nailgun __instance)
            => BuildAndSendWeaponFire(2, 3, (byte)__instance.variation);

        [DynamicPatch(typeof(Nailgun), "ShootZapper")]
        [Postfix]
        public static void OnNailgunShootZapper(Nailgun __instance)
            => BuildAndSendWeaponFire(2, 4, (byte)__instance.variation);
    }
}