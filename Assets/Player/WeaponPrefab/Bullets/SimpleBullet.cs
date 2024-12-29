using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleBullet : Bullet
{
    private Rigidbody2D myBody;
    [DoNotSerialize] public bool isPlayerBullet = true;

    public void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        myBody.velocity = transform.up * movementSpeed;
    }

    public void FixedUpdate()
    {
        range -= Time.fixedDeltaTime * movementSpeed;
        if (range < 0) Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wall")) Destroy(gameObject);
        if (isPlayerBullet && !collision.gameObject.CompareTag("Monster")) return;

        var target = collision.gameObject.GetComponent<Monster>();
        target.TakeDamage(damages);
        Destroy(gameObject);
    }
}
