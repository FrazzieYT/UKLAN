using System.Collections.Generic;
using System.Linq;
using UltraLANCoop.Network.Entities;

namespace UltraLANCoop.Network
{
    public static class PlayerManager
    {
        private static readonly Dictionary<int, PlayerData> _players = new Dictionary<int, PlayerData>();
        public static event System.Action<int> OnPlayerJoined;
        public static event System.Action<int> OnPlayerLeft;
        public static void AddPlayer(int id, string name, bool isLocal, bool isHost, int ping = 0)
        {
            _players[id] = new PlayerData
            {
                Id = id,
                Name = name,
                IsLocal = isLocal,
                IsHost = isHost,
                HP = GetMaxHP(),
                MaxHP = GetMaxHP(),
                Ping = ping
            };

            if (!isLocal)
                OnPlayerJoined?.Invoke(id);
        }
        public static void RemovePlayer(int id)
        {
            _players.Remove(id);
            OnPlayerLeft?.Invoke(id);
        }
        public static void Clear()
        {
            _players.Clear();
        }
        public static bool TryGetPlayer(int id, out PlayerData data)
        {
            return _players.TryGetValue(id, out data);
        }
        public static void UpdatePlayerHP(int id, int hp, int maxHp)
        {
            if (_players.TryGetValue(id, out var p))
            {
                p.HP = hp;
                p.MaxHP = maxHp;
            }
        }
        public static PlayerData[] GetAllPlayers() => _players.Values.ToArray();
        public static int GetMaxHP()
        {
            try
            {
                var difficulty = PrefsManager.Instance.GetInt("difficulty", 2);
                switch (difficulty)
                {
                    case 0: return 200; // Harmless
                    case 1: return 150; // Lenient
                    case 2: return 100; // Standard
                    case 3: return 80;  // Violent
                    case 4: return 60;  // Brutal
                    case 5: return 50;  // Very Hard
                    default: return 100;
                }
            }
            catch
            {
                return 100;
            }
        }
    }
}