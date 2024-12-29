using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Centiped : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed;
    [Min(0f)] public float steeringForce;
    [Min(0.01f)] public float spacing = 1f;
    [Range(1, 10.0f)] public int PathPrecision = 1;

    [Header("Comportement")]
    public float chargingZone;
    public float restingZone;
    public float fleeingZone;
    public float noFleeingAngle;

    [Header("Other")]
    [Min(0)] public int partCount;
    [Min(0)] public float partsHealth;
    public GameObject targettedEntity;
    public GameObject partPrefab;

    // Stuff
    [HideInInspector] public Rigidbody2D myBody;
    [HideInInspector] public List<CentipedPart> BodyParts = new();

    public void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
        ResetWaypoints();
    }

    // ------------------------------------------------------------- BEHAVIOUR

    private Vector2 targetPosition;

    private int state = 0;
    private const int CHARGING = 0;
    private const int TURNING = 1;
    private const int FLEEING = 2;
    public void FixedUpdate()
    {
        targetPosition = targettedEntity.transform.position;
        float targetAngle = Vector2.SignedAngle(transform.up, (targetPosition - myBody.position).normalized);

        if (state == TURNING)
        {
            float rotation = Mathf.Min(Mathf.Abs(targetAngle), steeringForce * Time.deltaTime) * Mathf.Sign(targetAngle);
            myBody.rotation += rotation;
        }
        if (state == FLEEING)
        {
            float rotation = Mathf.Min(Mathf.Abs(targetAngle), steeringForce * Time.deltaTime) * Mathf.Sign(targetAngle);
            myBody.rotation -= rotation;
        }

        myBody.position += movementSpeed * Time.deltaTime * (Vector2)transform.up;
        UpdateBody();

        var distToTarget = Vector2.Distance(targetPosition, myBody.position);
        
        if (state == CHARGING && distToTarget >= chargingZone) state = TURNING;
        if (state == TURNING && distToTarget < fleeingZone) state = FLEEING;
        if (state == FLEEING && distToTarget >= restingZone) state = TURNING;

        if (state == CHARGING && Mathf.Abs(targetAngle) >= noFleeingAngle) state = FLEEING;
        if (state == FLEEING && Mathf.Abs(targetAngle) < noFleeingAngle) state = TURNING;
        if (Mathf.Abs(targetAngle) < 5f) state = CHARGING;
    }

    // ------------------------------------------------------------- MOVEMENT

    private readonly List<Vector2> waypoints = new();
    public void ResetWaypoints()
    {
        waypoints.Clear();
        for (int i = 0; i < BodyParts.Count - 1; i++)
        {
            var start = BodyParts[i].transform.position;
            var end = BodyParts[i + 1].transform.position;
            waypoints.Add(start);

            for (int j = 1; j < PathPrecision; j++)
            {
                var t = j / (float)(PathPrecision);
                waypoints.Add(Vector3.Lerp(start, end, t));
            }
        }
        waypoints.Add(BodyParts.Last().transform.position);
    }

    private void UpdateBody()
    {
        float headDist = (myBody.position - waypoints[0]).magnitude;
        float scaledSpacing = spacing * transform.localScale.y; // Can be wider without affecting the spacing

        // New waypoints
        while (headDist > (scaledSpacing / PathPrecision))
        {
            var pointPos = waypoints[0] + (myBody.position - waypoints[0]).normalized * (scaledSpacing / PathPrecision);
            waypoints.Insert(0, pointPos);
            waypoints.RemoveAt(waypoints.Count - 1);

            headDist = (myBody.position - waypoints[0]).magnitude;
        }

        // Body parts positions
        for (int i = 1; i < BodyParts.Count; i++)
        {
            var index = i * PathPrecision;
            var current = waypoints[index];
            var prev = waypoints[index - 1];

            BodyParts[i].myBody.position = Vector3.Lerp(current, prev, headDist / (scaledSpacing / PathPrecision));
            BodyParts[i].myBody.rotation = Vector2.SignedAngle(Vector2.up, prev - BodyParts[i].myBody.position);
        }
    }

    public void OnDrawGizmos()
    {
        foreach(Vector3 point in waypoints)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(point, 0.1f);
        }
    }

    // ------------------------------------------------------------- SPLITTING BEHAVIOUR

    public void NotifyPartDestroyed(CentipedPart destroyedPart)
    {
        int index = BodyParts.IndexOf(destroyedPart);
        if (index < 0) return;

        List<CentipedPart> headParts = BodyParts.GetRange(0, index);
        List<CentipedPart> tailParts = (index < BodyParts.Count - 1) 
            ? BodyParts.GetRange(index + 1, BodyParts.Count - (index + 1))
            : new();

        if (tailParts.Count > 0) {
            Centiped newHead = Utils.CopyComponent(this, tailParts.First().gameObject);
            newHead.BodyParts = tailParts;

            foreach (CentipedPart part in newHead.BodyParts)
            {
                part.head = newHead;
            }
        }
        BodyParts = headParts;
    }

    // ------------------------------------------------------------- Creation
    [ContextMenu("Generate body parts")]
    public void CreateBodyParts()
    {
        BodyParts.Clear();
        CentipedPart part = gameObject.GetComponent<CentipedPart>() ?? gameObject.AddComponent<CentipedPart>();
        part.head = this;
        part.health = partsHealth;
        BodyParts.Add(part);

        for (int i = 0; i < partCount; i++) {
            part = Instantiate(partPrefab, transform.position, transform.rotation).GetComponent<CentipedPart>();
            part.head = this;
            part.health = partsHealth;
            BodyParts.Add(part);
        }
    }

    // ------------------------------------------------------------- Wall collision

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            // Reflection formula :
            // w = v - 2 * (v.n) * n 

            var normal = collision.GetContact(0).normal;
            var dot = Vector2.Dot(normal, transform.up);
            if (dot >= 0) return;
            var reflection = (Vector2)transform.up - 2 * dot * normal;

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, reflection)));
            ResetWaypoints();
        }
    }


    /*
     * ARCHIVE
     * 
    private void SnapToPrev(bool recursive = true)
    {
        if (previousPart == null) return;

        Vector3 snapDir = (transform.position - previousPart.transform.position).normalized;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, -snapDir)));
        snapDir *= spacing;
        transform.position = previousPart.transform.position + snapDir;

        if (recursive && nextPart != null) nextPart.SnapToPrev();
    }
    */
}

