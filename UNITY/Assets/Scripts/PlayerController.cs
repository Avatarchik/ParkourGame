using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public Transform cameraHeight { private set; get; }
    public Animator cameraAnimation;

    PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        cameraHeight = transform.Find("CameraHeight");
    }

    void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        cameraAnimation.SetFloat("velocity", movement.velocityMagnitude, 0.2f, Time.smoothDeltaTime);
    }
}
