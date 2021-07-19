using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Target to follow")]
    public GameObject Target;
    [Tooltip("The maximum distance the camera is allowed to move away from the Target")]
    public float MaxDistance = 4.0f;
    [Tooltip("The speed at which the Camera will try to keep up with the target")]
    public float Speed = 1.0f;
    [Tooltip("Offset that will be added onto the Target position")]
    public Vector3 CameraOffset = new Vector3(0, 2.0f, -2.5f);
    [Tooltip("Horizontal rotation speed in degrees per second")]
    public float HorizontalRotationSpeed = 1.0f;
    [Tooltip("Vertical rotation speed in degrees per second")]
    public float VerticalRotationSpeed = 1.0f;
    [Tooltip("Time before the camera will start to move back to its default position")]
    public float CameraResetCooldown = 3.0f;
    [Tooltip("Time it takes for the camera to move back to its default position")]
    public float CameraResetTime = 1.0f;

    private Transform currentTarget;
    private Camera mainCamera;
    private float horizontalAngle = 0;
    private float verticalAngle = 0;
    private Vector3 rotatedCameraOffset;
    private Quaternion currentAngle = Quaternion.identity;
    private Quaternion angleCheckpoint = Quaternion.identity;

    private float horizontalInput = 0;
    private float verticalInput = 0;

    private bool IsResetingCamera = false;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        currentTarget = Target.transform;
        rotatedCameraOffset = CameraOffset;
    }

    void Debug()
    {
        DebugHUD.Instance.PrintVariable("RotationTimer", rotationTimer);
        DebugHUD.Instance.PrintVariable("Vertical Angle", verticalAngle);

        Vector2 input = new Vector2(horizontalInput, verticalInput);
        DebugHUD.Instance.PrintVariable("Rotation Horizontal/Vertical", input);
    }

    public void ShiftView(float HorizontalDegrees, float VerticalDegrees)
    {
        horizontalAngle += HorizontalDegrees;
        horizontalAngle = ((horizontalAngle + 180.0f) % 360.0f) - 180.0f;

        verticalAngle += VerticalDegrees;
        verticalAngle = ((verticalAngle + 180.0f) % 360.0f) - 180.0f;

        currentAngle = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
        
        rotatedCameraOffset = currentAngle * CameraOffset;
    }

    public void SetViewAngle(float HorizontalAngle, float VerticalAngle)
    {
        horizontalAngle = HorizontalAngle;
        verticalAngle = VerticalAngle;

        currentAngle = Quaternion.Euler(0, HorizontalAngle, VerticalAngle);

        rotatedCameraOffset = currentAngle * CameraOffset;
    }

    public void SetViewAngle(Quaternion Angle)
    {
        verticalAngle = Angle.eulerAngles.x;
        horizontalAngle = Angle.eulerAngles.y;

        currentAngle = Angle;
        rotatedCameraOffset = currentAngle * CameraOffset;
    }

    public void SetCheckpoint(Quaternion Angle)
    {
        angleCheckpoint = Angle;
    }

    void LookAtTarget(Transform target)
    {
        currentTarget = target;
    }

    void LookAtTarget(GameObject target)
    {
        currentTarget = target.transform;
    }

    void FollowTarget()
    {
        Vector3 targetPosition = Target.transform.position + rotatedCameraOffset;
        float distance = Vector3.Distance(targetPosition, transform.position);

        float error = 0;
        if (distance > MaxDistance)
            error = distance - MaxDistance;
        else
            error = distance * Time.deltaTime * Speed;

        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();

        Vector3 temp = direction * error;
        transform.Translate(temp, Space.World);
    }

    void LookAtTarget()
    {
        transform.LookAt(currentTarget);
    }

    Coroutine temp;
    public void ResetCamera(float Delay = 0.0f)
    {
        //make sure only one resetcamera action is active
        if (temp != null)
            StopCoroutine(temp);

        IsResetingCamera = true;
        rotationTimer = CameraResetCooldown + Delay;
        temp = StartCoroutine(ResetCameraCorotine(CameraResetTime, Delay));
    }

    IEnumerator ResetCameraCorotine(float time, float delay = 0.0f)
    {
        float timer = 0;

        //delay if necessary
        if(delay > 0.0f)
        {
            while (timer < delay)
            {
                if (!IsResetingCamera)
                    yield break;

                timer += Time.deltaTime;
                yield return null; ;
            }
            timer = 0;
        }

        //reset
        //checkpoint because currentAngle will change during the reset
        Quaternion checkpoint = currentAngle;

        while (timer < time)
        {
            if (!IsResetingCamera)
                yield break;

            SetViewAngle(Quaternion.Slerp(checkpoint, angleCheckpoint, timer / time));
            timer += Time.deltaTime;
            yield return null;
        }

        IsResetingCamera = false;
        //extra assurance that the new camera offset is correct
        rotatedCameraOffset = angleCheckpoint * CameraOffset;
    }

    private float rotationTimer = 0;

    void CameraRotationCheck()
    {
        horizontalInput = Input.GetAxis("Rotation Horizontal");
        verticalInput = Input.GetAxis("Rotation Vertical");

        if (Mathf.Abs(horizontalInput) > 0.0f || Mathf.Abs(verticalInput) > 0.0f)
        {
            ShiftView(horizontalInput * HorizontalRotationSpeed * Time.deltaTime,
                      verticalInput * VerticalRotationSpeed * Time.deltaTime);

            rotationTimer = CameraResetCooldown;
            IsResetingCamera = false;
        }
        else if(rotationTimer <= 0 && !IsResetingCamera)
        {
            ResetCamera();
        }

        rotationTimer -= Time.deltaTime;
    }


    void Update()
    {

        CameraRotationCheck();

        FollowTarget();
        LookAtTarget();

#if UNITY_EDITOR
        Debug();
#endif
    }
}
