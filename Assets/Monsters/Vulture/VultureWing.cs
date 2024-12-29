using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VultureWing : MonoBehaviour
{
    [Header("Constraints")]
    [Min(1)] public int jointCount;
    [Min(0.01f)] public float spacing = 1f;
    public AnimationCurve angleConstraint;
    [Min(0.01f)] public float featherAngleConstraint;

    // Stuff
    public LineRenderer Body = new();
    public GameObject FeatherTemplate;
    private List<GameObject> Feathers = new();

    public void FixedUpdate()
    {
        UpdateWing();
    }

    public void UpdateWing()
    {
        if (Body.positionCount != jointCount + 1) ResetWing();

        Body.SetPosition(0, transform.position);

        Vector2 prevUp = transform.up;
        for (int i = 1; i < Body.positionCount; i++) { 
            Vector3 part = Body.GetPosition(i);
            Vector3 prevPart = Body.GetPosition(i - 1);

            // Snap the part to the right distance
            Vector3 snapDir = (part - prevPart).normalized;
            snapDir *= spacing;
            part = prevPart + snapDir;

            // Apply the angle constraint
            float angleDifference = Vector2.SignedAngle(prevUp, snapDir);
            if (Mathf.Abs(angleDifference) > angleConstraint.Evaluate((float)i / (float)jointCount))
            {
                float rotationAngle = Mathf.Abs(angleDifference) - angleConstraint.Evaluate((float)i / (float)jointCount);
                rotationAngle *= -Mathf.Sign(angleDifference);
                part = Utils.RotateAround(part, prevPart, rotationAngle, Vector3.forward);
            }
            
            // Body is done
            Body.SetPosition(i, part);
            prevUp = (part - prevPart).normalized;

            // Feathers
            var width = Body.widthCurve.Evaluate((float)i / (float)jointCount) / 2.0f;
            Feathers[i - 1].transform.position = part + Quaternion.AngleAxis(90, -Vector3.forward) * prevUp * width;
            Vector2 featherUp = Quaternion.AngleAxis(90, -Vector3.forward) * prevUp;
            angleDifference = Vector2.SignedAngle(featherUp, Feathers[i - 1].transform.rotation * Vector2.up);
            if (Mathf.Abs(angleDifference) > featherAngleConstraint)
            {
                float rotationAngle = Mathf.Abs(angleDifference) - featherAngleConstraint;
                rotationAngle *= -Mathf.Sign(angleDifference);
                Feathers[i - 1].transform.Rotate(new Vector3(0,0, rotationAngle));
            }
        }
    }

    public void ResetWing()
    {
        foreach (GameObject feather in Feathers)
        {
            Destroy(feather);
        }
        Feathers.Clear();

        if(jointCount < 1) jointCount = 1;

        Body.positionCount = jointCount + 1;

        for (int i = 0; i < jointCount + 1; i++) {
            Body.SetPosition(i, transform.position + transform.up * i);

            if(i > 0)
            {
                Feathers.Add(Instantiate(FeatherTemplate, transform.position + transform.up * i, Quaternion.AngleAxis(90, Vector3.forward)));
            }
        }
    }
}
