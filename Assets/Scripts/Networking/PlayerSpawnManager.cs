using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    private int nextSpawnIndex = 0;

    private void Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        Vector3 spawnPosition = GetNextSpawnPoint();
        Quaternion spawnRotation = Quaternion.identity;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.transform.position = spawnPosition;
                client.PlayerObject.transform.rotation = spawnRotation;
            }
        }
    }

    private Vector3 GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return Vector3.zero;
        }

        Vector3 position = spawnPoints[nextSpawnIndex].position;
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
        return position;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}

