namespace UltraLANCoop.Network.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LiteNetLib.Utils;
    using UnityEngine;

    public static class EntityPool
    {
        private static readonly Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        private static int nextEntityId = 1;

        public static int Count => entities.Count;

        public static void Add(Entity entity)
        {
            if (!entities.ContainsKey(entity.Id))
            {
                entities[entity.Id] = entity;
                entity.Create();
            }
        }

        public static void Remove(int entityId)
        {
            if (entities.TryGetValue(entityId, out Entity entity))
            {
                entities.Remove(entityId);
                entity.Kill(-1);
            }
        }

        public static Entity Get(int entityId)
        {
            entities.TryGetValue(entityId, out Entity entity);
            return entity;
        }

        public static List<T> GetAll<T>() where T : Entity
        {
            return entities.Values.OfType<T>().ToList();
        }

        public static int RegisterEntity() => nextEntityId++;

        public static void UpdateAll(float deltaTime)
        {
            foreach (var entity in entities.Values) entity.Update(deltaTime);
        }

        public static void SendSnapshots()
        {
            if (!NetCore.IsServer) return;

            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.Snapshot);
            writer.Put(entities.Count);

            foreach (var entity in entities.Values)
            {
                writer.Put(entity.Id);
                writer.Put((byte)entity.Type);
                entity.Write(writer);
            }

            NetCore.SendToAll(writer, LiteNetLib.DeliveryMethod.Unreliable);
        }

        public static void HandleSnapshot(NetDataReader reader)
        {
            if (NetCore.IsServer) return;

            int count = reader.GetInt();

            for (int i = 0; i < count; i++)
            {
                int entityId = reader.GetInt();
                EntityType type = (EntityType)reader.GetByte();

                Entity entity = Get(entityId);

                if (entity == null)
                {
                    switch (type)
                    {
                        case EntityType.Player:
                            entity = new PlayerEntity(entityId, entityId);
                            break;
                        case EntityType.Enemy:
                            entity = new EnemyEntity(entityId);
                            break;
                        default:
                            continue;
                    }
                    Add(entity);
                }

                entity.Read(reader);
            }
        }

        public static void Clear()
        {
            foreach (var entity in entities.Values) entity.Kill(-1);
            entities.Clear();
            nextEntityId = 1;
        }
    }
}