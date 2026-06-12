using HarmonyLib;
using System.Linq;
using UnityEngine;
using UltraLANCoop.Network;
using UltraLANCoop.Network.Entities;
using UltraLANCoop.Harmony;

namespace UltraLANCoop.Patches
{
    public static class EnemyPatch
    {
        [DynamicPatch(typeof(Enemy), "GetHurt",
            typeof(GameObject), typeof(Vector3), typeof(float), typeof(float),
            typeof(Vector3), typeof(GameObject), typeof(bool))]
        [Postfix]
        static void OnEnemyDamage(Enemy __instance, GameObject target, Vector3 force, float multiplier, float critMultiplier)
        {
            if (!NetCore.IsConnected) return;

            var eid = __instance.GetComponent<EnemyIdentifier>();
            if (eid == null) return;

            var entity = EntityPool.GetAll<EnemyEntity>()
                .FirstOrDefault(e => e.EnemyId == eid.GetInstanceID());

            if (entity == null && NetCore.IsServer)
            {
                entity = new EnemyEntity(EntityPool.Count + 1000);
                entity.AssignEnemy(__instance, eid.GetInstanceID());
                EntityPool.Add(entity);
            }

            if (entity != null)
            {
                Debug.Log($"[EnemyPatch] Enemy damage: {multiplier}");
                NetCore.SendEnemyDamage(entity.Id, multiplier, critMultiplier);
            }
        }

        [DynamicPatch(typeof(Enemy), "GoLimp", typeof(bool))]
        [Postfix]
        static void OnEnemyDeath(Enemy __instance, bool fromExplosion)
        {
            if (!NetCore.IsConnected) return;

            var eid = __instance.GetComponent<EnemyIdentifier>();
            if (eid == null) return;

            var entity = EntityPool.GetAll<EnemyEntity>()
                .FirstOrDefault(e => e.EnemyId == eid.GetInstanceID());

            if (entity != null)
            {
                Debug.Log($"[EnemyPatch] Enemy death");
                NetCore.SendEnemyDeath(entity.Id, fromExplosion);
            }
        }
    }
}