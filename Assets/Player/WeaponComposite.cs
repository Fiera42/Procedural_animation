using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponComposite : IWeapon
{
    public List<IWeapon> weapons = new();
    public CompositeFiringMode firingMode;
    private int index = 0;

    public override void Setup()
    {
        foreach (IWeapon weapon in weapons)
        {
            weapon.myController = myController;
            weapon.Setup();
        }
    }

    public override void Fire()
    {
        cooldown -= Time.deltaTime;
        if (isRoot && cooldown > 0) return;
        cooldown = fireRate;

        switch(firingMode)
        {
            case CompositeFiringMode.Sequential:
                SequentialFire();
                break;
            case CompositeFiringMode.Simultaneous:
                SimultaneousFire();
                break;
            case CompositeFiringMode.Random:
                RandomFire();
                break;
        }
    }

    private void SequentialFire()
    {
        if (weapons.Count == 0) return;

        weapons[index].Fire();
        index = ++index % weapons.Count;
    }

    private void SimultaneousFire()
    {
        foreach (IWeapon weapon in weapons)
        {
            weapon.Fire();
        }
    }

    private void RandomFire()
    {
        if (weapons.Count == 0) return;

        weapons[index].Fire();
        index = Random.Range(0, weapons.Count);
    }
}
