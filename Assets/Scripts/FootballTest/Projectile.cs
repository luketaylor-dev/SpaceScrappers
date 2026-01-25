using Unity.Netcode;
using UnityEngine;

namespace Football
{
    [RequireComponent(typeof(Collider))]
    public class Projectile : NetworkBehaviour
    {
        private ProjectileDropper dropper;
        private NetworkObject networkObject;

        private void Awake()
        {
            networkObject = GetComponent<NetworkObject>();
        }

        public void Initialize(ProjectileDropper dropper)
        {
            this.dropper = dropper;
        }


        public void DestroyProjectile()
        {
            if (networkObject != null && networkObject.IsSpawned)
            {
                DestroyProjectileServerRpc();
            }
            else
            {
                if (dropper != null)
                {
                    dropper.OnProjectileDestroyed(gameObject);
                }
                Destroy(gameObject);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DestroyProjectileServerRpc()
        {
            if (dropper != null)
            {
                dropper.OnProjectileDestroyed(gameObject);
            }

            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
        }
    }
}
