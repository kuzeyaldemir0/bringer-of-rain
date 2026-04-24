using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    private Transform target;
    private Rigidbody2D targetBody;
    private Vector3 offset;
    private float followSpeed;
    private Vector2 xLimits;
    private bool clampX;
    private float lookAheadX;

    public void Configure(Transform followTarget, Vector3 followOffset, float smoothing, Vector2 horizontalLimits)
    {
        target = followTarget;
        targetBody = followTarget != null ? followTarget.GetComponent<Rigidbody2D>() : null;
        offset = followOffset;
        followSpeed = smoothing;
        xLimits = horizontalLimits;
        clampX = true;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        if (targetBody != null)
        {
            lookAheadX = Mathf.Lerp(lookAheadX, Mathf.Clamp(targetBody.linearVelocity.x * 0.18f, -1.2f, 1.2f), 1f - Mathf.Exp(-7f * Time.deltaTime));
            desired.x += lookAheadX;
            desired.y += Mathf.Clamp(targetBody.linearVelocity.y * 0.05f, -0.5f, 0.5f);
        }

        desired.z = offset.z;

        if (clampX)
        {
            desired.x = Mathf.Clamp(desired.x, xLimits.x, xLimits.y);
        }

        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
    }
}
