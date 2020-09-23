using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovementController : MonoBehaviour
{
    // Start is called before the first frame update
    float horizontal;
    float vertical;

    public float DecelerationTick = 0.01f;
    public float AccelerationTick = 0.01f;
    public float MaxSpeed = 1.0f;

    private Vector2 direction = new Vector2();
    private float speed = 0;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void GetInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void UpdateController()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", speed / MaxSpeed);
    }

    void debug()
    {
        Debug.Log(new Vector2(horizontal, vertical));
        Debug.Log(direction.magnitude);
        Debug.Log(speed);
    }

    void Update()
    {
        GetInput();

        if (horizontal != 0.0f || vertical != 0.0f)
        {
            speed = Mathf.Min(MaxSpeed, speed + AccelerationTick);
            direction += new Vector2(horizontal, vertical);
            direction.Normalize();
        }
        else
        {
            speed = Mathf.Max(0,speed - DecelerationTick);
        }

        direction *= speed * Time.deltaTime;

        transform.Translate(new Vector3(direction.x, 0, direction.y));

        UpdateController();

#if UNITY_EDITOR
        debug();
#endif
    }
}
