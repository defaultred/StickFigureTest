using UnityEngine;

/// <summary>
/// Spin an object uniformously overtime.
/// </summary>
public class Spin : MonoBehaviour
{
    public Vector3 Spinning = Vector3.zero;

    private void Update() { transform.localRotation = Quaternion.Euler(Spinning.x * Time.deltaTime, Spinning.y * Time.deltaTime, Spinning.z * Time.deltaTime) * transform.localRotation; }
}


