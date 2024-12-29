using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class IWeapon : MonoBehaviour
{
    public Controller myController;
    [SerializeField] protected bool isRoot = false;
    [SerializeField] protected bool isPrimary = true;
    [SerializeField] protected float fireRate = 0f;
    protected float cooldown = 0f;

    protected virtual void Start()
    {
        if (!isRoot) return;

        if (myController == null)
        {
            Debug.LogError(gameObject + ": No controller found");
            gameObject.SetActive(false);
        }

        Setup();
    }

    protected virtual void Update()
    {
        bool shouldFire = (isPrimary && myController.IsFiringPrimary) || (!isPrimary && myController.IsFiringSecondary);
        if (isRoot && shouldFire) Fire();
    }

    public abstract void Setup();
    public abstract void Fire();
}
