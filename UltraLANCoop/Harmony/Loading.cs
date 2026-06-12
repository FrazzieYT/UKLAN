namespace UltraLANCoop.Harmony
{
    using UltraLANCoop.Network;
    using UnityEngine;

    public static class Loading
    {
        [StaticPatch(typeof(SceneHelper), nameof(SceneHelper.LoadSceneAsync))]
        [StaticPatch(typeof(SceneHelper), nameof(SceneHelper.RestartSceneAsync))]
        [Postfix]
        static void Load()
        {
            Events.OnLoadingStart?.Invoke();
        }

        [DynamicPatch(typeof(FinalRank), nameof(FinalRank.LevelChange))]
        [Prefix]
        static bool After()
        {
            if (NetCore.IsServer) return true;

            Debug.Log("[LOAD] Level change blocked for client");
            // Показать сообщение "load-mission" через UI
            return false;
        }

        [DynamicPatch(typeof(AbruptLevelChanger), nameof(AbruptLevelChanger.AbruptChangeLevel))]
        [Prefix]
        static bool Other() => After();

        [DynamicPatch(typeof(AbruptLevelChanger), nameof(AbruptLevelChanger.GoToSavedLevel))]
        [Prefix]
        static bool Saved() => After();
    }
}