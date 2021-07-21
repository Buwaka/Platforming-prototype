using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedMovementController : MonoBehaviour
{

    [Tooltip("Deceleration of the character, unit/second")]
    public float DecelerationTick = 0.1f;
    [Tooltip("Acceleration of the character, unit/second")]
    public float AccelerationTick = 0.01f;
    [Tooltip("Max speed that the controller can reach, unit/second")]
    public float MaxSpeed = 0.3f;
    [Tooltip("between 0 and 1, what amount of input change is necessary to force a camera reset")]
    public float HardTurnThreshold = 0.3f;
    [Tooltip("Threshold where input is not considered 'movement', this is useful for when to set a new movement angle")]
    public float MovementCutOff = 0.4f;
    [Tooltip("Character model to apply rotation and animation parameters to to")]
    public GameObject CharacterModel;
    [Tooltip("Object that stays behind the controller to dictate the direction")]
    public Transform Anchor;
    [Tooltip("Cameracontroller that follows this object")]
    public CameraController CameraController;


    //internal variables
    private Vector3 inputDirection = new Vector3();
    private Vector3 anchorDirection = new Vector3();
    private Vector3 direction = new Vector3();
    private float speed = 0;
    private Animator animator;
    private float horizontal, lastHorizontal;
    private float vertical, lastVertical;
    private float directionMagnitude = 0;
    private Transform cameraTransform;

    //flags
    private bool isMoving;


    // Start is called before the first frame update
    void Start()
    {
        animator = CharacterModel.GetComponent<Animator>();
        cameraTransform = CameraController.transform;
    }

    bool IsHardTurn()
    {
        if ((Mathf.Abs(horizontal - lastHorizontal) > HardTurnThreshold) || (Mathf.Abs(vertical - lastVertical) > HardTurnThreshold))
            return true;

        return false;
    }

    void GetInput()
    {
        lastHorizontal = horizontal;
        lastVertical = vertical;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        inputDirection = new Vector3(horizontal, 0, vertical);
        inputDirection.Normalize();

        if (Mathf.Abs(horizontal) > MovementCutOff ||
            Mathf.Abs(vertical) > MovementCutOff)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    void UpdateAnimationController()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", speed);
    }

    void debug()
    {
        DebugHUD.Instance.PrintVariable("Horizontal/Vertical", new Vector2(horizontal, vertical));
        DebugHUD.Instance.PrintVariable("Local Direction", direction);
        //DebugHUD.Instance.PrintVariable("World Direction", transform.TransformDirection(direction));
        DebugHUD.Instance.PrintVariable("Speed", speed);
        DebugHUD.Instance.PrintVariable("Hardturn", IsHardTurn());
        DebugHUD.Instance.PrintVariable("Anchor Direction", anchorDirection);
    }

    public void SetDesiredAnchor(Transform newAnchor)
    {
        cameraTransform = newAnchor;
    }

    public void ResetAnchor()
    {
        //do this in a smooth lerp or something
        //also check if transform objects are referenced and not copied by value
        Anchor.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
        CameraController.SetCheckpoint();
    }


    #region Operational

    void CalculateSpeed()
    {
        bool hardTurn = IsHardTurn();
        directionMagnitude = Mathf.Abs(horizontal) + Mathf.Abs(vertical) / 2;

        if (directionMagnitude > MovementCutOff && !hardTurn)
        {
            //acceleration
            speed = Mathf.Min(MaxSpeed, speed + (AccelerationTick * Time.deltaTime * directionMagnitude));
        }
        else
        {
            if (hardTurn)
            {
                //Accelerate in the opposite direction
                speed = Mathf.Max(0, speed - (AccelerationTick * Time.deltaTime * directionMagnitude));
            }
            else
            {
                //decceleration
                speed = Mathf.Max(0, speed - (DecelerationTick * Time.deltaTime));
            }
        }
    }

    void CalculateDirection()
    {
        anchorDirection = Anchor.forward;



        direction = Anchor.rotation * inputDirection;
        direction.y = 0;
        direction.Normalize();
    }

    void Transform()
    {
        if (speed != 0.0f)
        {
            transform.Translate(direction * speed);

        }

        if(directionMagnitude > 0.0f)
        {
            CharacterModel.transform.localRotation = Quaternion.LookRotation(direction, CharacterModel.transform.up);
        }


    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if (!isMoving)
            ResetAnchor();

        CalculateSpeed();
        CalculateDirection();

        Transform();

        UpdateAnimationController();

#if UNITY_EDITOR
        debug();
#endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + inputDirection);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + anchorDirection);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + direction);

        //Gizmos.color = Color.magenta;
        //Gizmos.DrawSphere(Anchor.position, 0.5f);
    }
}
