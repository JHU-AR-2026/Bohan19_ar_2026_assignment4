using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameManager game;
    public float speed = 5f;
    public float boostedSpeed = 8f;
    public float jumpForce = 7f;

    Rigidbody body;
    Vector3 movement;
    Vector3 startPosition;
    bool jumpRequested;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        startPosition = transform.position;
        body.useGravity = false;
    }

    void Update()
    {
        if (!game.started || game.won)
            return;

        float x = 0f;
        float z = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.W)) z += 1f;
        movement = new Vector3(x, 0f, z).normalized;

        bool grounded = Physics.Raycast(transform.position, Vector3.down, 0.6f, 1 << 6);
        if (Input.GetKeyDown(KeyCode.Space) && grounded && body.linearVelocity.y <= 0.1f)
            jumpRequested = true;
    }

    void FixedUpdate()
    {
        if (!game.started || game.won)
            return;

        float currentSpeed;
        if (game.powerActive)
            currentSpeed = boostedSpeed;
        else
            currentSpeed = speed;
        body.linearVelocity = new Vector3(movement.x * currentSpeed, body.linearVelocity.y,
            movement.z * currentSpeed);

        if (jumpRequested)
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }

        if (body.position.y < -5f)
        {
            body.position = startPosition;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!game.started || game.won)
            return;

        if (other.CompareTag("Collectible"))
        {
            other.gameObject.SetActive(false);
            game.CollectCube();
        }
        else if (other.CompareTag("PowerUp"))
        {
            other.gameObject.SetActive(false);
            game.powerActive = true;
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (other.CompareTag("Goal"))
        {
            game.won = true;
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (game.powerActive && collision.gameObject.CompareTag("BreakableWall"))
            collision.gameObject.SetActive(false);
    }
}

