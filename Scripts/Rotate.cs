using UnityEngine;

public class Rotate : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    public enum RotationDirection
    {
        Clockwise,
        Anticlockwise
    }

    [Header("Rotation Settings")]
    [Tooltip("Axis to rotate around")]
    public RotationAxis rotationAxis = RotationAxis.Y;

    [Tooltip("Direction of rotation")]
    public RotationDirection direction = RotationDirection.Clockwise;

    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 90f;

    [Header("Optional Settings")]
    [Tooltip("Use unscaled time (ignores Time.timeScale)")]
    public bool useUnscaledTime = false;

    void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        
        // Calculate rotation amount
        float rotationAmount = rotationSpeed * deltaTime;
        
        // Apply direction
        if (direction == RotationDirection.Anticlockwise)
        {
            rotationAmount = -rotationAmount;
        }

        // Create rotation vector based on selected axis
        Vector3 rotationVector = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X:
                rotationVector = new Vector3(rotationAmount, 0, 0);
                break;
            case RotationAxis.Y:
                rotationVector = new Vector3(0, rotationAmount, 0);
                break;
            case RotationAxis.Z:
                rotationVector = new Vector3(0, 0, rotationAmount);
                break;
        }

        // Apply rotation
        transform.Rotate(rotationVector, Space.Self);
    }
}
