using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCameraOverlayUI : MonoBehaviour
{
    public PlayerController playerController;
    public TextMeshProUGUI hitCountTMP;

    public void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController != null && hitCountTMP != null)
        {
            hitCountTMP.text = $"HitCount: {playerController.hitCount}";
        }
    }
}
