using HarmonyLib;
using UnityEngine;
using UltraLANCoop.Network;
using UltraLANCoop.Network.Entities;
using UltraLANCoop.Harmony;

namespace UltraLANCoop.Patches
{
    public static class PlayerSyncPatch
    {
        private static bool _isProcessingNetworkDamage = false;
        private static float _lastSyncTime = 0f;
        private const float SYNC_INTERVAL = 0.05f;

        [DynamicPatch(typeof(NewMovement), "Start")]
        [Postfix]
        public static void OnStart(NewMovement __instance)
        {
            if (!NetCore.IsConnected) return;

            var localPlayer = EntityPool.Get(NetCore.LocalPlayerId) as PlayerEntity;
            if (localPlayer != null)
            {
                localPlayer.PlayerTransform = __instance.transform;
                localPlayer.PlayerRigidbody = __instance.rb;
                Debug.Log("[PlayerSyncPatch] ✅ Local player initialized");
            }
        }

        [DynamicPatch(typeof(NewMovement), "Update")]
        [Postfix]
        public static void OnUpdate(NewMovement __instance)
        {
            if (!NetCore.IsConnected || NetCore.IsServer) return;
            if (Time.time - _lastSyncTime < SYNC_INTERVAL) return;
            _lastSyncTime = Time.time;

            var localPlayer = EntityPool.Get(NetCore.LocalPlayerId) as PlayerEntity;
            if (localPlayer == null) return;

            if (__instance.gc != null)
                localPlayer.IsGrounded = __instance.gc.onGround;

            localPlayer.IsSliding = __instance.sliding;
            localPlayer.IsDashing = __instance.boost;
            localPlayer.IsJumping = __instance.jumping;
            localPlayer.IsFalling = __instance.falling;
            localPlayer.IsWalking = __instance.walking;

            if (__instance.rb != null)
                localPlayer.Velocity = __instance.rb.velocity;
        }

        [DynamicPatch(typeof(NewMovement), "GetHurt")]
        [Prefix]
        public static void GetHurtPrefix(NewMovement __instance, ref int __state)
        {
            __state = __instance.hp;
        }

        [DynamicPatch(typeof(NewMovement), "GetHurt")]
        [Postfix]
        public static void GetHurtPostfix(NewMovement __instance, int __state)
        {
            if (_isProcessingNetworkDamage || !NetCore.IsConnected) return;

            int deltaHp = __state - __instance.hp;
            if (deltaHp > 0)
            {
                Debug.Log($"[PlayerSyncPatch] 🩸 Урон: {deltaHp}");
                NetCore.SendDamage(NetCore.LocalPlayerId, deltaHp, -1);
                NetCore.UpdatePlayer(NetCore.LocalPlayerId, __instance.hp, NetCore.GetMaxHP());
            }
        }

        [DynamicPatch(typeof(NewMovement), "Jump")]
        [Postfix]
        public static void OnJump(NewMovement __instance)
        {
            if (!NetCore.IsConnected) return;
            var localPlayer = EntityPool.Get(NetCore.LocalPlayerId) as PlayerEntity;
            if (localPlayer != null)
            {
                localPlayer.IsJumping = true;
                localPlayer.IsFalling = true;
            }
        }

        [DynamicPatch(typeof(NewMovement), "StartSlide")]
        [Postfix]
        public static void OnStartSlide(NewMovement __instance)
        {
            if (!NetCore.IsConnected) return;
            var localPlayer = EntityPool.Get(NetCore.LocalPlayerId) as PlayerEntity;
            if (localPlayer != null) localPlayer.IsSliding = true;
        }

        [DynamicPatch(typeof(NewMovement), "StopSlide")]
        [Postfix]
        public static void OnStopSlide(NewMovement __instance)
        {
            if (!NetCore.IsConnected) return;
            var localPlayer = EntityPool.Get(NetCore.LocalPlayerId) as PlayerEntity;
            if (localPlayer != null) localPlayer.IsSliding = false;
        }

        [DynamicPatch(typeof(NewMovement), "TryDash")]
        [Postfix]
        public static void OnTryDash(NewMovement __instance)
        {
            if (!NetCore.IsConnected) return;
            var localPlayer = EntityPool.Get(NetCore.LocalPlayerId) as PlayerEntity;
            if (localPlayer != null) localPlayer.IsDashing = true;
        }
    }
}