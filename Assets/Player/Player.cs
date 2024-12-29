using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Controller))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [HideInInspector] public Controller myController;
    private Rigidbody2D myBody;

    public void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        myController = GetComponent<Controller>();
    }

    public void Update()
    {
        if(myController.MovementDir == Vector2.zero)
        {
            myBody.velocity = Vector2.zero;
        } 
        else
        {
            myBody.velocity = movementSpeed * myController.MovementDir.normalized;
        }

        if (myController.AttackDir != Vector2.zero)
        {
            transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, myController.AttackDir));
        }
    }
}
