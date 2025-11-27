using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class playerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Sprite y Animaciones")]
    [SerializeField] private GameObject spriteObject;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private CharacterController controller;
    private Vector3 velocity;

    private ClientPlayerMove net;

    public Vector3 move;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        TryGetComponent(out net);

        if (spriteObject != null)
        {
            spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
            animator = spriteObject.GetComponent<Animator>();
        }
        else
        {
            Debug.LogWarning("spriteObject no asignado en playerMovement.");
        }
    }

    private void Update()
    {
        if (net == null || !net.IsOwner)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        move = transform.right * x + transform.forward * z;

        if (move.magnitude > 1f)
            move.Normalize();

        bool isWalking = (x != 0 || z != 0);

        // Si camina no puede estar sentado
        if (isWalking && net.IsSitting.Value)
            net.IsSitting.Value = false;

        // --- Manejo de Sitting ---
        if (!isWalking && Input.GetKeyDown(KeyCode.M))
        {
            net.IsSitting.Value = !net.IsSitting.Value; // Toggle sentado
        }

        // Sincronizar walking
        net.IsWalking.Value = isWalking;

        // Movimiento solo si NO está sentado
        if (!net.IsSitting.Value)
        {
            controller.Move(move * speed * Time.deltaTime);
        }

        // Gravedad
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (!net.IsSitting.Value && Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Flip
        if (x > 0)
            net.LookDirection.Value = 1;
        else if (x < 0)
            net.LookDirection.Value = -1;
    }

    // ============================
    // ACTUALIZAR ANIMADOR
    // ============================
    public void UpdateAnimator(bool walking, bool sitting)
    {
        if (animator == null)
            return;

        animator.SetBool("Walking", walking);
        animator.SetBool("Sitting", sitting);
    }

    // ============================
    // ACTUALIZAR FLIP
    // ============================
    public void UpdateFlip(int lookDir)
    {
        if (spriteObject == null)
            return;

        if (lookDir == 1)
            spriteObject.transform.localScale = new Vector3(5, 5, 5);
        else if (lookDir == -1)
            spriteObject.transform.localScale = new Vector3(-5, 5, 5);
    }
}
