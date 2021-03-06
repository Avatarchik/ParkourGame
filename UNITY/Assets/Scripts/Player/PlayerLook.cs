﻿using UnityEngine;
using System.Collections;
using Rewired;

public class PlayerLook : MonoBehaviour 
{
    public Transform lookX;
    public Transform lookY;
    public float sensitivity = 5;

    Player playerInput;
    Transform transform;

    Vector2 localRotation;

    void Awake()
    {
        playerInput = Rewired.ReInput.players.GetPlayer(0);
        transform = GetComponent<Transform>();
    }

    void Update()
    {
        localRotation += new Vector2(playerInput.GetAxisRaw("Look Vertical"), playerInput.GetAxisRaw("Look Horizontal")) * sensitivity;
        localRotation.x = Mathf.Clamp(localRotation.x, -80, 80);

        lookX.localEulerAngles = new Vector3(localRotation.x, 0, 0);
        lookY.localEulerAngles = new Vector3(0, localRotation.y, 0);
    }
}
