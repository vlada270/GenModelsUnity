using UnityEngine;

public class RotationY : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    [SerializeField] private Axis rotationAxis = Axis.Y;

    [SerializeField, Range(-200f, 200f)]
    private float rotationSpeed = 50f;

    private void Update()
    {
        Vector3 axis =
            rotationAxis == Axis.X ? Vector3.right :
            rotationAxis == Axis.Y ? Vector3.up :
                                     Vector3.forward;

        transform.Rotate(axis, rotationSpeed * Time.deltaTime);
    }
}
