using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInterval : IWeapon
{
    [SerializeField] GameObject myBullet;
    [SerializeField] float firingAngle = 0;
    [SerializeField] float firingOffset = 0;
    [SerializeField] bool randomizeOffset = false;

    private float currentOffset = 0;

    public override void Setup()
    {
        if (myBullet == null)
        {
            Debug.LogError(gameObject + ": No bullet found");
            gameObject.SetActive(false);
        }
    }

    public override void Fire()
    {
        cooldown -= Time.deltaTime;
        if (isRoot && cooldown > 0) return;
        cooldown += fireRate;

        float dir;

        if(randomizeOffset)
        {
            dir = Random.Range(0, firingAngle);
        }
        else
        {
            dir = (currentOffset + firingOffset) % firingAngle;
            currentOffset = dir;
        }

        dir -= firingAngle / 2f;
        dir += Vector2.SignedAngle(Vector2.up, transform.up);

        var bullet = Instantiate(myBullet, transform.position, Quaternion.Euler(0, 0, dir));
    }
}
