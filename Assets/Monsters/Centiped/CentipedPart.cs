using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CentipedPart : Monster
{
    public Centiped head;
    [HideInInspector] public Rigidbody2D myBody;

    public void Start()
    {
        if (head == null)
        {
            Debug.LogError(gameObject + ": missing centiped head reference");
            gameObject.SetActive(false);
        }
        myBody = GetComponent<Rigidbody2D>();
    }

    public override void TakeDamage(float damage)
    {
        health -= damage;
        if(health < 0)
        {
            head.NotifyPartDestroyed(this);
            Destroy(gameObject);
        }
    }
}
