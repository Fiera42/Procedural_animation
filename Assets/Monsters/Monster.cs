using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    public float health;
    public abstract void TakeDamage(float damage);
}
