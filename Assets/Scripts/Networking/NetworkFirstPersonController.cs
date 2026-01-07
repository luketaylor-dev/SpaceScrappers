using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkFirstPersonController : NetworkBehaviour
{
    [SerializeField] private FirstPersonController localController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Canvas playerUI;
    
    [Header("Character Model")]
    [SerializeField] private GameObject characterModel;
    [SerializeField] private Renderer[] characterRenderers;

    private void Awake()
    {
        if (localController == null)
            localController = GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        if (transform.position.y < -10)
        {
            transform.position = NetworkManager.Singleton.transform.position;
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
    }
}

