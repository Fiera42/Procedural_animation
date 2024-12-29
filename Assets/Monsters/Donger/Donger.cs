using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Donger : Monster
{
    [Header("Characteristics")]
    public float rotationSpeed;
    public float windupTime;
    public float squeezeSpeed;
    public float idleTime;
    public float extendSpeed;
    public float maxSpacing;
    public float randomRange;

    [Header("Other")]
    public Rigidbody2D leftShell;
    public Rigidbody2D rightShell;
    public LineRenderer link;
    public BoxCollider2D attackTrigger;
    public GameObject shellPrefab;
    public GameObject targettedEntity;

    public void Start()
    {
        if (leftShell == null)
        {
            Debug.LogError(gameObject + ": No leftShell found");
            gameObject.SetActive(false);
        }
        if (rightShell == null)
        {
            Debug.LogError(gameObject + ": No leftShell found");
            gameObject.SetActive(false);
        }
        if (link == null)
        {
            Debug.LogError(gameObject + ": No link found");
            gameObject.SetActive(false);
        }

        currentSqueeze = maxSpacing;
    }

    // ------------------------------------------------------------- BEHAVIOUR

    [Range(-1, 1)] private int leftRotationDir = 1;
    [Range(-1, 1)] private int rightRotationDir = -1;
    [Min(0.01f)] private float currentSqueeze;
    private float timer;

    private int state = LEFT_TURNING;
    private const int IDLE = 0;
    private const int LEFT_TURNING = 1;
    private const int RIGHT_TURNING = 2;
    private const int WINDUP = 3;
    private const int SQUEEZING = 4;
    private const int EXTENDING = 5;

    public void FixedUpdate()
    {
        // Go forward
        if (state == LEFT_TURNING || state == RIGHT_TURNING) 
        {
            var vect = (state == LEFT_TURNING)
                ? (leftShell.position - (Vector2)transform.position).normalized
                : (rightShell.position - (Vector2)transform.position).normalized;

            var angle = Vector2.SignedAngle(transform.up, vect);

            if(Mathf.Abs(angle) < 5f) swapPart();
        }
        
        if(state == WINDUP || state == IDLE)
        {
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                state = (state == WINDUP) ? SQUEEZING : EXTENDING;
            }
        }

        if (state == SQUEEZING) if (squeeze())
            {
                state = IDLE;
                timer = idleTime;
            }
        if (state == EXTENDING) if (extend()) state = LEFT_TURNING;
        if (state == LEFT_TURNING || state == RIGHT_TURNING) updateRotation();

        updateMyPosition();
        updateLink();
        updateAttackTrigger();
    }

    public void swapRotation()
    {
        leftRotationDir = -leftRotationDir;
        rightRotationDir = -rightRotationDir;
    }
    public void swapPart()
    {
        if (state == LEFT_TURNING) state = RIGHT_TURNING;
        else if (state == RIGHT_TURNING) state = LEFT_TURNING;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            state = WINDUP;
            timer = windupTime;
        }
    }

    public override void TakeDamage(float damage)
    {
        health -= damage;
        if(health < 0)
        {
            Destroy(leftShell.gameObject);
            Destroy(rightShell.gameObject);
            Destroy(gameObject);
        }
    }

    // ------------------------------------------------------------- MOVEMENT

    private bool squeeze()
    {
        currentSqueeze -= squeezeSpeed * Time.deltaTime;
        currentSqueeze = Mathf.Clamp(currentSqueeze, 0.01f, maxSpacing);
        setSpaceShells(currentSqueeze);
        return currentSqueeze == 0.01f;
    }

    private bool extend()
    {
        currentSqueeze += extendSpeed * Time.deltaTime;
        currentSqueeze = Mathf.Clamp(currentSqueeze, 0.01f, maxSpacing);
        setSpaceShells(currentSqueeze);
        return currentSqueeze == maxSpacing;
    }

    private void updateRotation()
    {
        // Δθ = ΔS/r
        var angle = rotationSpeed / maxSpacing;

        if(state == LEFT_TURNING)
        {
            Utils.RotateAround(leftShell, rightShell.position, angle * leftRotationDir);
        }
        else if(state == RIGHT_TURNING)
        {
            Utils.RotateAround(rightShell, leftShell.position, angle * rightRotationDir);
        }
        rightShell.rotation = Vector2.SignedAngle(Vector2.up, leftShell.position - rightShell.position);
        leftShell.rotation = Vector2.SignedAngle(Vector2.up, rightShell.position - leftShell.position);
    }

    private void setSpaceShells(float dist)
    {
        var dir = (leftShell.position - (Vector2)transform.position).normalized;
        leftShell.position = dir * dist/2 + (Vector2)transform.position;

        dir = (rightShell.position - (Vector2)transform.position).normalized;
        rightShell.position = dir * dist/2 + (Vector2)transform.position;

        currentSqueeze = dist;
    }

    private void updateAttackTrigger()
    {
        var myScale = attackTrigger.transform.localScale;
        myScale.y = Mathf.Max(leftShell.transform.localScale.x, rightShell.transform.localScale.x);
        attackTrigger.transform.localScale = myScale;

        var leftToRight = leftShell.position - rightShell.position;

        var mySize = attackTrigger.size;
        mySize.x = leftToRight.magnitude / 2;
        attackTrigger.size = mySize;

        attackTrigger.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, leftToRight));
    }

    public void notifyCollision(Collision2D collision)
    {
        /* Random movement behaviour
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Reflection formula :
            // w = v - 2 * (v.n) * n 
            var normal = collision.GetContact(0).normal;
            var dot = Vector2.Dot(normal, transform.up);
            if (dot == 0) dot = 0.001f;

            var reflection = (Vector2)transform.up - 2 * dot * normal;
            var angle = Vector2.SignedAngle(Vector2.up, reflection);
            angle = angle + (Random.value * randomRange - randomRange/2);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            swapRotation();
        }
        */

        if (collision.gameObject.CompareTag("Wall"))
        {
            var angle = Vector2.SignedAngle(Vector2.up, targettedEntity.transform.position - transform.position);
            angle = angle + (Random.value * randomRange - randomRange / 2);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            swapRotation();
        }
    }

    private void updateMyPosition()
    {
        transform.position = (rightShell.position + leftShell.position)/2;
    }

    private void updateLink()
    {
        link.SetPositions(
            new[] { (Vector3)leftShell.position, (Vector3)rightShell.position }
            );
    }

    // ------------------------------------------------------------- Creation
    [ContextMenu("Generate body parts")]
    public void generateBodyPart()
    {
        rightShell = Instantiate(shellPrefab, transform.position + transform.right, Quaternion.Euler(new Vector3(0, 0, 0)))
            .GetComponent<Rigidbody2D>();
        leftShell = Instantiate(shellPrefab, transform.position - transform.right, Quaternion.Euler(new Vector3(0, 0, 0)))
            .GetComponent<Rigidbody2D>();

        rightShell.GetComponent<DongerPart>().donger = this;
        leftShell.GetComponent<DongerPart>().donger = this;

        var dir = (leftShell.position - (Vector2)transform.position).normalized;
        leftShell.transform.position = dir * maxSpacing / 2 + (Vector2)transform.position;

        dir = (rightShell.position - (Vector2)transform.position).normalized;
        rightShell.transform.position = dir * maxSpacing / 2 + (Vector2)transform.position;

        currentSqueeze = maxSpacing;

        link.positionCount = 2;
        updateLink();
    }
}