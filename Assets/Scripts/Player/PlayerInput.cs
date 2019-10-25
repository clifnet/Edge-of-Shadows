using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]

public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public float horizontal;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool crouchHeld;
    [HideInInspector] public bool crouchPressed;
    [HideInInspector] public bool attack;
    [HideInInspector] public bool evade;
    [HideInInspector] public static bool restart;
    [HideInInspector] public static bool switchWorld;

    internal bool horizontalAccess = true;

    bool _readyToClear;

    void Update()
    {
        ClearInput();

        //if (GameManager.IsGameOver())
        //    return;

        ProcessInputs();

        if ((Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow)))
            horizontal = 0f;
        else
            horizontal = Mathf.Clamp(horizontal, -1f, 1f);

        _readyToClear = true;
    }

    //void FixedUpdate()
    //{
    //    _readyToClear   = true;
    //}

    void ClearInput()
    {
        if (!_readyToClear)
            return;
        
        horizontal      = 0f;
        jumpHeld        = false;
        jumpPressed     = false;
        crouchHeld      = false;
        crouchPressed   = false;
        evade           = false;

        restart         = false;

        switchWorld     = false;
        
        _readyToClear   = false;
    }

    void ProcessInputs()
    {
        horizontal      = Input.GetAxis("Horizontal");

        jumpPressed     = jumpPressed || Input.GetButtonDown("Jump");
        jumpHeld        = jumpHeld || Input.GetButton("Jump");

        crouchPressed   = crouchPressed || Input.GetButtonDown("Crouch");
        crouchHeld      = crouchHeld || Input.GetButton("Crouch");

        evade           = evade || Input.GetButtonDown("Evade");
        
        //Making delay after press for combo attacks
        if (Input.GetButtonDown("Attack"))
            attack      = true;

        switchWorld = switchWorld || Input.GetButtonDown("Switch World");

        restart = restart || Input.GetKeyDown(KeyCode.R);
    }
}
