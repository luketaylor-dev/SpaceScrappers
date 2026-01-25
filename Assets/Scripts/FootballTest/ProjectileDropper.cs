using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Football
{
    public class ProjectileDropper : NetworkBehaviour
    {
        public static ProjectileDropper Instance { get; private set; }

        [SerializeField] private Vector2 xBounds;
        [SerializeField] private Vector2 zBounds;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float dropInterval = 5f;
        [SerializeField] private int maxProjectiles = 10;

        private List<NetworkObject> activeProjectiles = new List<NetworkObject>();
        private Coroutine dropCoroutine;
        private bool isNetworked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Multiple ProjectileDropper instances found. Destroying duplicate.");
                Destroy(gameObject);
            }
        }


        public override void OnNetworkSpawn()
        {
            isNetworked = true;

            if (IsServer)
            {
                if (projectilePrefab == null)
                {
                    Debug.LogError("Projectile prefab is not assigned!");
                    return;
                }

                Debug.Log("ProjectileDropper: Starting in networked mode (Server)");
                dropCoroutine = StartCoroutine(DropProjectilesCoroutine());
            }
            else
            {
                Debug.Log("ProjectileDropper: Client - not starting coroutine");
            }
        }

        public override void OnDestroy()
        {
            if (dropCoroutine != null)
            {
                StopCoroutine(dropCoroutine);
            }

            if (Instance == this)
            {
                Instance = null;
            }
            base.OnDestroy();
        }

        private IEnumerator DropProjectilesCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(dropInterval);

                if (activeProjectiles.Count < maxProjectiles)
                {
                    DropProjectile();
                }
            }
        }

        private void DropProjectile()
        {
            if (isNetworked && !IsServer)
            {
                Debug.LogWarning("DropProjectile called on client - ignoring");
                return;
            }

            Debug.Log("Dropping Projectile");
            float randomX = Random.Range(xBounds.x, xBounds.y);
            float randomZ = Random.Range(zBounds.x, zBounds.y);
            Vector3 spawnPosition = new Vector3(randomX, 0, randomZ);

            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity, transform);
            NetworkObject networkObject = projectileInstance.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError("Projectile prefab does not have a NetworkObject component!");
                Destroy(projectileInstance);
                return;
            }

            if (isNetworked)
            {
                networkObject.Spawn();
            }

            Projectile projectileScript = projectileInstance.GetComponent<Projectile>();

            projectileScript.Initialize(this);

            if (networkObject != null)
            {
                activeProjectiles.Add(networkObject);
            }
        }

        public void OnProjectileDestroyed(GameObject projectile)
        {
            if (isNetworked && !IsServer)
                return;

            NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
            if (networkObject != null && activeProjectiles.Contains(networkObject))
            {
                activeProjectiles.Remove(networkObject);
                if (isNetworked && networkObject.IsSpawned)
                {
                    networkObject.Despawn();
                }
            }
        }
    }
}