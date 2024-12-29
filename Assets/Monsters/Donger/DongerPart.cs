using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DongerPart : Monster
{
    public Donger donger;
    public void OnCollisionEnter2D(Collision2D collision)
    {
        donger.notifyCollision(collision);
    }

    public override void TakeDamage(float damage)
    {
        donger.TakeDamage(damage);
    }
}
