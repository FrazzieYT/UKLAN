namespace UltraLANCoop
{
    using System;
    using UnityEngine;

    public class Events : MonoBehaviour
    {
        public static Events Instance { get; private set; }

        public static Action OnLoadingStart = () => { };
        public static Action OnLoad = () => { };
        public static Action OnMainMenuLoad = () => { };
        public static Action OnHandChange = () => { };

        public static Action EveryTick = () => { };
        public static Action EveryHalf = () => { };

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InvokeRepeating(nameof(Tick), 1f, 1f / 20f);
            InvokeRepeating(nameof(Half), 1f, 0.5f);
        }

        private void Tick() => EveryTick();
        private void Half() => EveryHalf();

        private void OnLevelWasInitialized(int level)
        {
            OnLoad();
            if (level == 0) OnMainMenuLoad();
        }
    }
}