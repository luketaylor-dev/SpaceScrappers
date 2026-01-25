using System;
using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Networking
{
    public class NetworkFirstPersonController : NetworkBehaviour
    {
        [SerializeField] private FirstPersonController localController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;
        [SerializeField] private Canvas playerUI;
    
        [Header("Character Model")]
        [SerializeField] private GameObject characterModel;
        [SerializeField] private Renderer[] characterRenderers;

        [Header("Fall Detection")]
        [SerializeField] private float fallThreshold = -10f;
        [SerializeField] private Vector3 respawnPosition = Vector3.zero;

        private Rigidbody playerRigidbody;

        private void Awake()
        {
            if (localController == null)
                localController = GetComponent<FirstPersonController>();

            playerRigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (transform.position.y < fallThreshold)
            {
                RespawnPlayer();
            }
        }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            DisableLocalComponents();
            ShowCharacterModel();
        }
        else
        {
            EnableLocalComponents();
            HideCharacterModel();
        }
    }

    private void DisableLocalComponents()
    {
        if (localController != null)
            localController.enabled = false;

        if (playerCamera != null)
            playerCamera.enabled = false;

        if (audioListener != null)
            audioListener.enabled = false;

        if (playerUI != null)
            playerUI.enabled = false;
    }

    private void EnableLocalComponents()
    {
        if (localController != null)
            localController.enabled = true;

        if (playerCamera != null)
            playerCamera.enabled = true;

        if (audioListener != null)
            audioListener.enabled = true;

        if (playerUI != null)
            playerUI.enabled = true;
    }

    private void HideCharacterModel()
    {
        if (characterModel != null)
            characterModel.SetActive(false);

        if (characterRenderers != null)
        {
            foreach (var renderer in characterRenderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }
    }

    private void ShowCharacterModel()
    {
        if (characterModel != null)
            characterModel.SetActive(true);

        if (characterRenderers != null)
        {
            foreach (var renderer in characterRenderers)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
        }

        private void RespawnPlayer()
        {
            transform.position = respawnPosition;

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }

            RespawnPlayerClientRpc();
        }

        [ClientRpc]
        private void RespawnPlayerClientRpc()
        {
            if (IsOwner)
            {
                Debug.Log("Player respawned after falling");
            }
        }
    }
}

