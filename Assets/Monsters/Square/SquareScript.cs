using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SquareScript : Monster
{
    [Header("Characteristics")]
    public float movementSpeed;
    public float handGrabDist;
    public float handSpeed;
    public AnimationCurve handCurve;

    [Header("Other")]
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject targettedEntity;

    // ------------------------------------------------------------- BEHAVIOUR

    private Rigidbody2D myBody;
    private Vector2 target;
    private Vector2 originalRightHandPosition;
    private Vector2 originalLeftHandPosition;
    private float rndOffset;

    public void Start()
    {
        if (leftHand == null)
        {
            Debug.LogError(gameObject + ": No leftHand found");
            gameObject.SetActive(false);
        }
        if (rightHand == null)
        {
            Debug.LogError(gameObject + ": No rightHand found");
            gameObject.SetActive(false);
        }
        if (targettedEntity == null)
        {
            Debug.LogError(gameObject + ": No targetted entity found");
            gameObject.SetActive(false);
        }

        myBody = GetComponent<Rigidbody2D>();
        originalLeftHandPosition = leftHand.transform.localPosition;
        originalRightHandPosition = rightHand.transform.localPosition;
        rndOffset = Random.value;
    }

    public void FixedUpdate()
    {
        target = targettedEntity.transform.position;
        var vect = target - myBody.position;
        myBody.rotation = Vector2.SignedAngle(Vector2.up, vect);
        myBody.velocity = vect.normalized * movementSpeed;
    }

    public void Update()
    {
        float leftValue = (rndOffset + Time.time * handSpeed) % 2;
        float rightValue = (rndOffset + 1 + Time.time * handSpeed) % 2;

        leftValue = handCurve.Evaluate(leftValue);
        rightValue = handCurve.Evaluate(rightValue);

        leftHand.transform.localPosition = Vector2.Lerp(originalLeftHandPosition, originalLeftHandPosition + Vector2.up * handGrabDist, leftValue);
        rightHand.transform.localPosition = Vector2.Lerp(originalRightHandPosition, originalRightHandPosition + Vector2.up * handGrabDist, rightValue);
    }

    public override void TakeDamage(float damage)
    {
        health -= damage;
        if (health < 0)
        {
            Destroy(gameObject);
        }
    }
}
