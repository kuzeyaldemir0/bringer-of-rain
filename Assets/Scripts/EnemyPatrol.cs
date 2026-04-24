using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyPatrol : MonoBehaviour, IWaterReactive
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Windup,
        Dash,
        Recover,
        Stunned
    }

    private Rigidbody2D body;
    private PlayerController player;
    private SpriteRenderer spriteRenderer;

    private float leftBound;
    private float rightBound;
    private float speed;
    private float direction = 1f;
    private float knockbackVelocity;
    private float stateEndsAt;
    private float nextDashAt;
    private float nextTouchTime;

    private int currentHealth;
    private EnemyState currentState;

    private const int MaxHealth = 5;
    private const int ContactDamage = 1;
    private const float ChaseRange = 6.8f;
    private const float AttackRange = 2.35f;
    private const float VerticalAwareness = 2f;
    private const float ChaseMultiplier = 1.45f;
    private const float DashSpeed = 9.2f;
    private const float WindupDuration = 0.4f;
    private const float DashDuration = 0.34f;
    private const float RecoverDuration = 0.48f;
    private const float StunDuration = 0.3f;
    private const float DashCooldown = 1.05f;
    private static readonly Color BaseColor = Color.white;
    private static readonly Color AlertColor = new(1f, 0.94f, 0.86f, 1f);
    private static readonly Color DashColor = new(1f, 0.74f, 0.74f, 1f);
    private static readonly Color HurtColor = new(0.74f, 0.95f, 1f, 1f);

    public bool IsMoving => currentState == EnemyState.Patrol || currentState == EnemyState.Chase || currentState == EnemyState.Dash;
    public bool IsWindingUp => currentState == EnemyState.Windup;
    public bool IsDashing => currentState == EnemyState.Dash;
    public bool IsStunned => currentState == EnemyState.Stunned;
    public bool IsAlert => currentState == EnemyState.Chase || currentState == EnemyState.Windup || currentState == EnemyState.Dash;

    public void Configure(float minX, float maxX, float moveSpeed)
    {
        leftBound = Mathf.Min(minX, maxX);
        rightBound = Mathf.Max(minX, maxX);
        speed = moveSpeed;
    }

    public void ReactToWaterBurst(WaterBurstData burst)
    {
        if (currentHealth <= 0)
        {
            return;
        }

        int damage = burst.Damage;
        if (currentState == EnemyState.Windup || currentState == EnemyState.Dash)
        {
            damage += 1;
        }

        currentHealth -= damage;
        direction = burst.Direction.x > 0f ? -1f : 1f;
        knockbackVelocity = burst.Direction.x * 7f;
        currentState = EnemyState.Stunned;
        stateEndsAt = Time.time + StunDuration;
        spriteRenderer.color = HurtColor;

        if (currentHealth <= 0)
        {
            WaterBurstVisual.Spawn(transform.position, new Vector2(1.4f, 1f), burst.Direction.x);
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.size = new Vector2(0.7f, 0.9f);

        spriteRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        spriteRenderer.color = BaseColor;
        spriteRenderer.sortingOrder = 9;

        currentHealth = MaxHealth;
        currentState = EnemyState.Patrol;
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        Vector2 position = body.position;

        switch (currentState)
        {
            case EnemyState.Stunned:
                position.x += knockbackVelocity * Time.deltaTime;
                knockbackVelocity = Mathf.MoveTowards(knockbackVelocity, 0f, 28f * Time.deltaTime);
                if (Time.time >= stateEndsAt)
                {
                    currentState = CanChasePlayer() ? EnemyState.Chase : EnemyState.Patrol;
                }
                break;

            case EnemyState.Windup:
                if (Time.time >= stateEndsAt)
                {
                    currentState = EnemyState.Dash;
                    stateEndsAt = Time.time + DashDuration;
                }
                break;

            case EnemyState.Dash:
                position.x += direction * DashSpeed * Time.deltaTime;
                if (Time.time >= stateEndsAt)
                {
                    currentState = EnemyState.Recover;
                    stateEndsAt = Time.time + RecoverDuration;
                }
                break;

            case EnemyState.Recover:
                if (Time.time >= stateEndsAt)
                {
                    currentState = CanChasePlayer() ? EnemyState.Chase : EnemyState.Patrol;
                }
                break;

            default:
                bool chasing = CanChasePlayer();
                currentState = chasing ? EnemyState.Chase : EnemyState.Patrol;

                if (chasing)
                {
                    float deltaToPlayer = player.transform.position.x - transform.position.x;
                    if (Mathf.Abs(deltaToPlayer) > 0.15f)
                    {
                        direction = Mathf.Sign(deltaToPlayer);
                    }

                    if (Mathf.Abs(deltaToPlayer) <= AttackRange && Time.time >= nextDashAt)
                    {
                        currentState = EnemyState.Windup;
                        stateEndsAt = Time.time + WindupDuration;
                        nextDashAt = Time.time + DashCooldown;
                    }
                    else
                    {
                        position.x += direction * speed * ChaseMultiplier * Time.deltaTime;
                    }
                }
                else
                {
                    position.x += direction * speed * Time.deltaTime;
                }
                break;
        }

        if (position.x <= leftBound)
        {
            position.x = leftBound;
            direction = 1f;
            if (currentState == EnemyState.Dash)
            {
                currentState = EnemyState.Recover;
                stateEndsAt = Time.time + RecoverDuration;
            }
        }
        else if (position.x >= rightBound)
        {
            position.x = rightBound;
            direction = -1f;
            if (currentState == EnemyState.Dash)
            {
                currentState = EnemyState.Recover;
                stateEndsAt = Time.time + RecoverDuration;
            }
        }

        body.MovePosition(position);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction > 0f ? 1f : -1f);
        transform.localScale = scale;

        Color targetColor = currentState switch
        {
            EnemyState.Windup => AlertColor,
            EnemyState.Dash => DashColor,
            EnemyState.Stunned => HurtColor,
            _ => BaseColor
        };
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * 14f);
    }

    private bool CanChasePlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 playerPosition = player.transform.position;
        return Mathf.Abs(playerPosition.y - transform.position.y) <= VerticalAwareness &&
               Mathf.Abs(playerPosition.x - transform.position.x) <= ChaseRange;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (Time.time < nextTouchTime)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerController hitPlayer))
        {
            nextTouchTime = Time.time + (currentState == EnemyState.Dash ? 0.5f : 0.8f);
            hitPlayer.TakeDamage(ContactDamage, transform.position);
        }
    }
}
