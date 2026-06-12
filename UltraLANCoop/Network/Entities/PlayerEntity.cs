namespace UltraLANCoop.Network.Entities
{
    using UnityEngine;
    using LiteNetLib.Utils;

    public class PlayerEntity : Entity
    {
        public int PlayerId { get; private set; }
        public string PlayerName { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public bool IsDead { get; set; }

        private Vector3 targetPosition;
        private Quaternion targetRotation = Quaternion.identity;
        private Vector3 velocity;
        private Vector3 smoothVelocity;
        private GameObject visualObject;

        public bool IsGrounded { get; set; }
        public bool IsSliding { get; set; }
        public bool IsDashing { get; set; }
        public bool IsJumping { get; set; }
        public bool IsFalling { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsWalking { get; set; }

        public Vector3 Velocity { get => velocity; set => velocity = value; }
        public Transform PlayerTransform { get; set; }
        public Rigidbody PlayerRigidbody { get; set; }

        public PlayerEntity(int id, int playerId) : base(id, EntityType.Player)
        {
            PlayerId = playerId;
            PlayerName = $"Player {playerId}";
            Health = 100f;
            MaxHealth = 100f;
            IsDead = false;
        }

        public override void Create()
        {
            if (PlayerId == NetCore.LocalPlayerId)
            {
                var nmov = MonoSingleton<NewMovement>.Instance;
                if (nmov != null)
                {
                    PlayerTransform = nmov.transform;
                    PlayerRigidbody = nmov.rb;
                    targetPosition = PlayerTransform.position;
                    targetRotation = PlayerTransform.rotation;
                    Plugin.Instance.Logger.LogInfo($"[PlayerEntity] Local player {PlayerId} linked to NewMovement at {targetPosition}");
                }
                return;
            }

            try
            {
                visualObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visualObject.name = $"RemotePlayer_{PlayerId}_Capsule";
                
                var serverNmov = MonoSingleton<NewMovement>.Instance;
                if (serverNmov != null)
                {
                    targetPosition = serverNmov.transform.position;
                    targetRotation = serverNmov.transform.rotation;
                }

                visualObject.transform.position = targetPosition;
                visualObject.transform.rotation = targetRotation;
                visualObject.transform.localScale = new Vector3(0.5f, 1.0f, 0.5f);

                var renderer = visualObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    float hue = (PlayerId * 0.618033988749895f) % 1f;
                    Color color = Color.HSVToRGB(hue, 0.8f, 1.0f);
                    mat.color = color;
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 0.5f);
                    renderer.material = mat;
                }

                var collider = visualObject.GetComponent<Collider>();
                if (collider != null) UnityEngine.Object.Destroy(collider);

                PlayerTransform = visualObject.transform;
                Plugin.Instance.Logger.LogInfo($"[PlayerEntity] ✅ Created capsule for remote player {PlayerId} at {targetPosition}");
            }
            catch (System.Exception e)
            {
                Plugin.Instance.Logger.LogError($"[PlayerEntity] ❌ Error creating capsule: {e.Message}");
            }
        }

        public override void Kill(int killerId)
        {
            if (!IsDead) 
            { 
                IsDead = true; 
                Health = 0f; 
            }

            if (visualObject != null)
            {
                UnityEngine.Object.Destroy(visualObject);
                visualObject = null;
                PlayerTransform = null;
                Plugin.Instance.Logger.LogInfo($"[PlayerEntity] 🗑️ Capsule for player {PlayerId} destroyed");
            }
        }

        public override void ApplyDamage(int damage, int sourceId)
        {
            if (IsDead) return;

            Health -= damage;
            if (Health <= 0f)
            {
                Health = 0f;
                IsDead = true;
                Kill(sourceId);
            }
        }

        public override void Update(float deltaTime)
        {
            if (NetCore.IsServer || PlayerId == NetCore.LocalPlayerId)
            {
                if (PlayerTransform != null)
                {
                    targetPosition = PlayerTransform.position;
                    targetRotation = PlayerTransform.rotation;
                    if (PlayerRigidbody != null) velocity = PlayerRigidbody.velocity;
                }
            }
            else
            {
                if (PlayerTransform != null && visualObject != null)
                {
                    PlayerTransform.position = Vector3.SmoothDamp(PlayerTransform.position, targetPosition, ref smoothVelocity, 0.1f);
                    PlayerTransform.rotation = Quaternion.Slerp(PlayerTransform.rotation, targetRotation, deltaTime * 10f);
                }
            }
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(targetPosition.x);
            writer.Put(targetPosition.y);
            writer.Put(targetPosition.z);

            writer.Put(targetRotation.x);
            writer.Put(targetRotation.y);
            writer.Put(targetRotation.z);
            writer.Put(targetRotation.w);

            writer.Put(velocity.x);
            writer.Put(velocity.y);
            writer.Put(velocity.z);

            writer.Put(Health);
            writer.Put(MaxHealth);
            writer.Put(IsDead);
            writer.Put(IsGrounded);
            writer.Put(IsSliding);
            writer.Put(IsDashing);
            writer.Put(IsJumping);
            writer.Put(IsFalling);
        }

        public override void Read(NetDataReader reader)
        {
            if (NetCore.IsServer) return;
            if (PlayerId == NetCore.LocalPlayerId) return;

            targetPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            targetRotation = new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            velocity = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());

            Health = reader.GetFloat();
            MaxHealth = reader.GetFloat();
            IsDead = reader.GetBool();
            IsGrounded = reader.GetBool();
            IsSliding = reader.GetBool();
            IsDashing = reader.GetBool();
            IsJumping = reader.GetBool();
            IsFalling = reader.GetBool();
        }
    }
}