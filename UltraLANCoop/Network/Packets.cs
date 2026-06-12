namespace UltraLANCoop.Network
{
    public enum PacketType : byte
    {
        Snapshot = 1,
        Damage = 2,
        Death = 3,
        WeaponFire = 4,
        WeaponSwitch = 5,
        EnemySpawn = 10,
        EnemyDamage = 11,
        EnemyDeath = 12,
        Punch = 20,
        HookState = 21,
        Shockwave = 22,
        Blast = 23,
        ShotgunExplosion = 24,
        HammerExplosion = 25,
        ProjectileSpawn = 30,
        ProjectileUpdate = 31,
        HitscanFire = 40,
        ChatMessage = 50,
        Kick = 60,
        PlayerInfo = 61
    }
}