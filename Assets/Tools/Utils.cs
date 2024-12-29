using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Utils
{
    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    // RotateAround for rigidBodies
    public static void RotateAround(Rigidbody2D rigidbody, Vector2 center, float angle)
    {
        var quaternion = Quaternion.AngleAxis(angle, Vector3.forward);
        rigidbody.position = (Vector2)(quaternion * (rigidbody.position - center)) + center;
        rigidbody.rotation = (rigidbody.transform.rotation * quaternion).eulerAngles.z;
    }

    // RotateAround for vectors
    public static Vector3 RotateAround(Vector3 vect, Vector3 center, float angle, Vector3 axis)
    {
        var quaternion = Quaternion.AngleAxis(angle, axis);
        return (quaternion * (vect - center)) + center;
    }
}
