using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
    public Animator cameraAnimation;

    PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
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
