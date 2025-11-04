using UnityEngine;

public class RotateOnContact : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up; // Axis to rotate (e.g., Vector3.up for Y)
    public float rotationAngle = 90f;         // Degrees to rotate
    public string playerTag = "Player";      // Tag of the player object

    [Header("Delay Settings")]
    public float delaySeconds = 0.5f; // Delay before rotation

    [Header("Target Object")]
    public Transform targetToRotate; // Assign the object to rotate in the inspector

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            StartCoroutine(RotateWithDelay());
        }
    }

    private System.Collections.IEnumerator RotateWithDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        if (targetToRotate != null)
        {
            Quaternion startRot = targetToRotate.rotation;
            Quaternion endRot = Quaternion.AngleAxis(rotationAngle, rotationAxis) * startRot;
            float duration = 0.5f; // Duration of the smooth rotation
            float elapsed = 0f;
            while (elapsed < duration)
            {
                targetToRotate.rotation = Quaternion.Lerp(startRot, endRot, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            targetToRotate.rotation = endRot;
        }
    }
}
