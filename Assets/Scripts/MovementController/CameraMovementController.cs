﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
    // Start is called before the first frame update

    //parameters
    [Tooltip("Camera to listen to, for viewpoint and direction")]
    public Camera CameraToListenTo;

    [Tooltip("Deceleration of the character, unit/second")]
    public float DecelerationTick = 0.1f;
    [Tooltip("Acceleration of the character, unit/second")]
    public float AccelerationTick = 0.01f;
    [Tooltip("Max speed that the controller can reach, unit/second")]
    public float MaxSpeed = 0.3f;
    [Tooltip("time in seconds before camera resets to idle position")]
    public float IdleCameraResetTime = 2.0f;
    [Tooltip("between 0 and 1, what amount of input change is necessary to force a camera reset")]
    public float HardTurnThreshold = 0.3f;
    [Tooltip("Character model to apply rotation and animation parameters to to")]
    public GameObject CharacterModel;

    //internal variables
    private Vector3 direction = new Vector3();
    private float speed = 0;
    private Animator animator;
    private float horizontal, lastHorizontal;
    private float vertical, lastVertical;
    private Camera mainCamera;
    private Quaternion viewCheckpoint;

    void Start()
    {
        horizontal = 0;
        vertical = 0;

        //get animation controller
        animator = CharacterModel.GetComponent<Animator>();

        //get the camera to listen to
        if (CameraToListenTo)
        {
            mainCamera = CameraToListenTo;
        }
        else
        {
            mainCamera = GetComponent<Camera>();
        }
        //if no camera is found, assert
        Debug.Assert(mainCamera, "No Camera attached to movementon controller: " + this.name);

        SetViewpoint();
    }

    //Camera position without height difference
    Vector3 GetRelativeCameraPosition(bool NoHeight)
    {
        Vector3 temp = transform.position - mainCamera.transform.position;

        if (NoHeight)
            return new Vector3(temp.x, 0, temp.z);
        else
            return temp;

    }

    void SetViewpoint()
    {
        //set viewpoint from which we take direction
        var view = GetRelativeCameraPosition(true);
        viewCheckpoint = Quaternion.LookRotation(view, Vector3.up);
    }

    void ResetCamera()
    {
        CameraController controller = mainCamera.GetComponent<CameraController>();
        controller.SetCheckpoint(Quaternion.Euler(0, CharacterModel.transform.rotation.eulerAngles.y, 0));

        controller.ResetCamera(IdleCameraResetTime);
    }

    void GetInput()
    {
        lastHorizontal = horizontal;
        lastVertical = vertical;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void UpdateAnimationController()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", speed / MaxSpeed);
    }

    void debug()
    {
        //Debug.Log(new Vector2(horizontal, vertical));
        //Debug.Log("Local Direction: " + direction.ToString());
        //Debug.Log("World Direction: " + transform.TransformDirection(direction).ToString());

        DebugHUD.Instance.PrintVariable("Horizontal/Vertical", new Vector2(horizontal, vertical));
        DebugHUD.Instance.PrintVariable("Local Direction", direction);
        DebugHUD.Instance.PrintVariable("World Direction", transform.TransformDirection(direction));
        DebugHUD.Instance.PrintVariable("Speed", speed);
        DebugHUD.Instance.PrintVariable("Hardturn", IsHardTurn());
    }

    bool IsHardTurn()
    {
        if ((Mathf.Abs(horizontal - lastHorizontal) > HardTurnThreshold) || (Mathf.Abs(vertical - lastVertical) > HardTurnThreshold))
            return true;

        return false;
    }

    bool IsReset = false;
    void CalculateViewpoint()
    {
        //if there's no input, reset the camera and its viewpoint
        if ((horizontal == 0.0f && vertical == 0.0f))
        {
            SetViewpoint();

            //prevent camera from being constantly reset
            if (!IsReset)
            {
                ResetCamera();
                IsReset = true;
            }
        }
        else if(IsHardTurn())
        {
            SetViewpoint();
            ResetCamera();
            IsReset = true;
        }
        else
        {
            IsReset = false;
        }
    }

    void CalculateDirection()
    {
        if (horizontal != 0.0f || vertical != 0.0f)
        {
            //update direction
            direction += viewCheckpoint * new Vector3(horizontal, 0, vertical);
            direction.Normalize();
        }
    }

    void CalculateSpeed()
    {
        bool hardTurn = IsHardTurn();
        if ((horizontal != 0.0f || vertical != 0.0f) && !hardTurn)
        {
            //acceleration
            speed = Mathf.Min(MaxSpeed, speed + (AccelerationTick * Time.deltaTime));
        }
        else
        {
            if (hardTurn)
            {
                //complete stop
                speed = 0;
            }
            else
            {
                //decceleration
                speed = Mathf.Max(0, speed - (DecelerationTick * Time.deltaTime));
            }
        }
    }

    void Transform()
    {
        //rotation
        CharacterModel.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        //translation
        //Fix deze shit
        if(speed != 0.0f)
        {
            var temp = transform.TransformDirection(direction);
            transform.Translate(direction * speed);
        }

    }

    void Update()
    {
        GetInput();

        CalculateViewpoint();

        CalculateSpeed();
        CalculateDirection();

        Transform();

        UpdateAnimationController();

#if UNITY_EDITOR
        debug();
#endif
    }
}
