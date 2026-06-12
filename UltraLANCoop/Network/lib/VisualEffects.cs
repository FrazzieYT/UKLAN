using System.Linq;
using UnityEngine;
using UltraLANCoop.Network.Entities;
using BepInEx.Logging;

namespace UltraLANCoop.Network
{
    public static class VisualEffects
    {
        private static ManualLogSource Log => Plugin.Instance.Logger;
        public static void PlayWeaponFireEffect(int playerId, byte weaponType, byte shotType, byte gunVariation, Vector3 position, Vector3 direction)
        {
            if (playerId < 0 || playerId > 100)
            {
                Log.LogWarning($"[VisualEffects] ⚠️ Ignoring effect for suspicious ID: {playerId}");
                return;
            }

            var playerEntity = EntityPool.GetAll<PlayerEntity>().FirstOrDefault(p => p.PlayerId == playerId);

            if (playerEntity == null || playerEntity.PlayerTransform == null)
            {
                Log.LogWarning($"[VisualEffects] Cannot find visual for player {playerId}");
                return;
            }

            if (weaponType == 0)
            {
                PlayRevolverBeam(shotType, position, direction);
            }

            Log.LogInfo($"[VisualEffects] Played effect for player {playerId}");
        }
        private static void PlayRevolverBeam(byte shotType, Vector3 position, Vector3 direction)
        {
            GameObject beamObj = new GameObject("RevolverBeamEffect");
            beamObj.transform.position = position;

            var lineRenderer = beamObj.AddComponent<LineRenderer>();
            lineRenderer.startWidth = shotType == 2 ? 0.15f : 0.08f;
            lineRenderer.endWidth = shotType == 2 ? 0.05f : 0.02f;
            lineRenderer.startColor = shotType == 2 ? Color.red : Color.yellow;
            lineRenderer.endColor = shotType == 2 ? Color.yellow : Color.white;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, position);
            lineRenderer.SetPosition(1, position + direction * 200f);

            UnityEngine.Object.Destroy(beamObj, shotType == 2 ? 0.5f : 0.3f);
        }
    }
}