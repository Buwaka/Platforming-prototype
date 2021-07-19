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
    public Transform DirectionAnchor;


    //internal variables
    private Vector3 direction = new Vector3();
    private float speed = 0;
    private Animator animator;
    private float horizontal, lastHorizontal;
    private float vertical, lastVertical;
    private Transform desiredAnchor;

    //flags
    private bool isMoving;


    // Start is called before the first frame update
    void Start()
    {
        animator = CharacterModel.GetComponent<Animator>();
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
        DebugHUD.Instance.PrintVariable("World Direction", transform.TransformDirection(direction));
        DebugHUD.Instance.PrintVariable("Speed", speed);
        DebugHUD.Instance.PrintVariable("Hardturn", IsHardTurn());
    }

    public void SetDesiredAnchor(Transform newAnchor)
    {
        desiredAnchor = newAnchor;
    }

    public void ResetAnchor()
    {
        //do this in a smooth lerp or something
        //also check if transform objects are referenced and not copied by value
        DirectionAnchor = desiredAnchor;
    }


    #region Operational

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

    void CalculateDirection()
    {
        direction = DirectionAnchor.forward;
    }

    void Move()
    {
        if (speed != 0.0f)
        {
            transform.Translate(direction * speed);
        }
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        GetInput();


        CalculateSpeed();
        CalculateDirection();

        Move();

        UpdateAnimationController();

#if UNITY_EDITOR
        debug();
#endif
    }
}
