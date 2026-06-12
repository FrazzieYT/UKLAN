namespace UltraLANCoop.Network.Entities
{
    using UnityEngine;
    using LiteNetLib.Utils;

    public class EnemyEntity : Entity
    {
        public int EnemyId { get; private set; }
        public Enemy Enemy { get; private set; }
        public EnemyIdentifier EID { get; private set; }

        public float Health { get; set; }
        public bool IsDead { get; set; }

        private Vector3 position;
        private Quaternion rotation;
        private Vector3 lastPosition;
        private Quaternion lastRotation;

        public EnemyEntity(int id) : base(id, EntityType.Enemy)
        {
        }

        public void AssignEnemy(Enemy enemy, int enemyId)
        {
            Enemy = enemy;
            EnemyId = enemyId;
            EID = enemy.GetComponent<EnemyIdentifier>();

            if (EID != null)
            {
                Health = EID.health;
                IsDead = EID.dead;
            }

            if (Enemy != null)
            {
                position = Enemy.transform.position;
                rotation = Enemy.transform.rotation;
                lastPosition = position;
                lastRotation = rotation;
            }
        }

        public override void Create()
        {
            // Создание визуального врага на клиенте
        }

        public override void Kill(int killerId)
        {
            if (Enemy != null && !IsDead)
            {
                IsDead = true;
                // Добавить визуальные эффекты смерти
            }
        }

        public override void ApplyDamage(int damage, int sourceId)
        {
            Health -= damage;
            if (Health <= 0f && !IsDead)
            {
                IsDead = true;
                Kill(sourceId);
            }
        }

        public override void Update(float deltaTime)
        {
            if (Enemy == null || Enemy.gameObject == null) return;

            if (NetCore.IsServer)
            {
                lastPosition = position;
                lastRotation = rotation;
                position = Enemy.transform.position;
                rotation = Enemy.transform.rotation;

                if (EID != null)
                {
                    Health = EID.health;
                    IsDead = EID.dead;
                }
            }
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(position.x);
            writer.Put(position.y);
            writer.Put(position.z);

            writer.Put(rotation.x);
            writer.Put(rotation.y);
            writer.Put(rotation.z);
            writer.Put(rotation.w);

            writer.Put(Health);
            writer.Put(IsDead);
        }

        public override void Read(NetDataReader reader)
        {
            if (NetCore.IsServer) return;

            lastPosition = position;
            lastRotation = rotation;

            position = new Vector3(
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat()
            );

            rotation = new Quaternion(
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat()
            );

            Health = reader.GetFloat();
            IsDead = reader.GetBool();

            if (Enemy != null && Enemy.gameObject != null)
            {
                Enemy.transform.position = position;
                Enemy.transform.rotation = rotation;
            }
        }
    }
}