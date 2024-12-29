using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Controller : MonoBehaviour
{
    // -------------------------- INTERFACE
    public Vector2 MovementDir { get; private set; }
    public Vector2 AttackDir { get; private set; }
    public bool IsFiringPrimary { get; private set; }
    public bool IsFiringSecondary { get; private set; }

    // -------------------------- FUNCTIONNAL

    private PlayerInput inputs;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction attackAction;
    private InputAction secondaryAction;
    private InputAction interactAction;
    private InputAction dashAction;
    private InputAction pauseAction;

    public void Awake()
    {
        inputs = GetComponent<PlayerInput>();

        moveAction = inputs.actions["Move"];
        lookAction = inputs.actions["Look"];
        attackAction = inputs.actions["Attack"];
        secondaryAction = inputs.actions["Secondary"];
        interactAction = inputs.actions["Interact"];
        dashAction = inputs.actions["Dash"];
        pauseAction = inputs.actions["Pause"];

        attackAction.performed += context => IsFiringPrimary = true;
        attackAction.canceled += context => IsFiringPrimary = false;
        secondaryAction.performed += context => IsFiringSecondary = true;
        secondaryAction.canceled += context => IsFiringSecondary = false;

        moveAction.performed += context =>
        {
            var vector = moveAction.ReadValue<Vector2>();
            if (vector.magnitude > 0.1) MovementDir = vector.normalized;
            else MovementDir = Vector2.zero;
        };
        moveAction.canceled += context => MovementDir = Vector2.zero;

        lookAction.performed += context => {
            if(inputs.currentControlScheme == "Keyboard&Mouse")
            {
                var screenPos = lookAction.ReadValue<Vector2>();
                var mousePos = Camera.main.ScreenToWorldPoint(screenPos);
                AttackDir = (mousePos - transform.position).normalized;
            }
            else
            {
                AttackDir = lookAction.ReadValue<Vector2>();
            }
            };
    }
}
