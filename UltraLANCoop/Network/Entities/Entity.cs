namespace UltraLANCoop.Network.Entities
{
    using LiteNetLib.Utils;
    using UnityEngine;

    // Структура для плавной интерполяции значений
    public struct Float
    {
        public float Prev, Next;
        private float prev, next;

        public float Init
        {
            get => prev;
            set { prev = next = Prev = Next = value; }
        }

        public void Set(float value)
        {
            bool jumped = Mathf.Abs(Next - Prev) > 10f;
            float delta = next - prev;

            Prev = Next;
            Next = value;

            prev = jumped ? Prev : next;

            if (jumped)
            {
                next = prev + delta;
            }
            else if (Mathf.Abs(Next - Prev) > 0.01f)
            {
                next = value + (Next - Prev) * 0.28f;
            }
            else if (Mathf.Abs(Next - next) > 0.32f)
            {
                prev = next = value;
            }
        }

        public readonly float GetAware(float delta)
        {
            return Mathf.Lerp(prev, next, delta * 20f);
        }

        public readonly float GetAngle(float delta)
        {
            return Mathf.LerpAngle(Prev, Next, delta * 20f);
        }
    }
    public abstract class Entity
    {
        public int Id { get; private set; }
        public EntityType Type { get; private set; }

        protected Entity(int id, EntityType type)
        {
            Id = id;
            Type = type;
        }

        public abstract void Create();
        public abstract void Kill(int killerId);
        public abstract void ApplyDamage(int damage, int sourceId);
        public abstract void Update(float deltaTime);
        public abstract void Write(NetDataWriter writer);
        public abstract void Read(NetDataReader reader);
    }

    public enum EntityType
    {
        Player,
        Enemy,
        Projectile,
        Item
    }
}