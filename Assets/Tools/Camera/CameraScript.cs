using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float smoothness;
    public float aimOffset;
    public float previewOffset;

    public Transform player;
    public Controller controller;

    private Vector2 currentOffset;

    public void Start()
    {
        if(player == null)
        {
            Debug.LogError(gameObject + ": missing player reference");
            gameObject.SetActive(false);
        }
        if (controller == null)
        {
            Debug.LogError(gameObject + ": missing controller reference");
            gameObject.SetActive(false);
        }
    }

    public void LateUpdate()
    {
        Vector3 targetPosition = new(player.position.x, player.position.y, transform.position.z);

        Vector2 targetOffset = aimOffset * controller.AttackDir.normalized;
        targetOffset += previewOffset * controller.MovementDir.normalized;

        currentOffset += (targetOffset - currentOffset) * Mathf.Min(1.0f, smoothness * Time.deltaTime);


        transform.position = targetPosition + (Vector3)currentOffset;
    }
}
