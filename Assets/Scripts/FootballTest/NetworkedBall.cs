using Unity.Netcode;
using UnityEngine;

namespace Football
{
    [RequireComponent(typeof(NetworkObject), typeof(Rigidbody))]
    public class NetworkedBall : NetworkBehaviour
    {
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                rb.isKinematic = true;
            }
        }
    }
}
