using UnityEngine;

public class IceSpearProjectile : MonoBehaviour
{
    private Vector2 direction;
    private LayerMask groundMask;
    private LayerMask targetMask;
    private GameObject source;
    private float speed;
    private float maxRange;
    private float force;
    private float traveled;
    private int damage;
    private bool hasHit;

    private const float HitRadius = 0.17f;
    private static readonly Color ShaftColor = new(0.62f, 0.95f, 1f, 0.96f);
    private static readonly Color CoreColor = new(0.94f, 1f, 1f, 1f);
    private static readonly Color TrailColor = new(0.24f, 0.78f, 1f, 0.42f);

    public static void Spawn(
        Vector2 position,
        Vector2 aimDirection,
        int damage,
        float force,
        float speed,
        float maxRange,
        LayerMask groundMask,
        LayerMask targetMask,
        GameObject source)
    {
        GameObject spearObject = new("IceSpearProjectile");
        spearObject.transform.position = new Vector3(position.x, position.y, -0.45f);

        IceSpearProjectile spear = spearObject.AddComponent<IceSpearProjectile>();
        spear.Initialize(aimDirection, damage, force, speed, maxRange, groundMask, targetMask, source);
    }

    private void Initialize(
        Vector2 aimDirection,
        int spearDamage,
        float spearForce,
        float spearSpeed,
        float spearMaxRange,
        LayerMask spearGroundMask,
        LayerMask spearTargetMask,
        GameObject spearSource)
    {
        direction = aimDirection.sqrMagnitude > 0.01f ? aimDirection.normalized : Vector2.right;
        damage = spearDamage;
        force = spearForce;
        speed = spearSpeed;
        maxRange = spearMaxRange;
        groundMask = spearGroundMask;
        targetMask = spearTargetMask;
        source = spearSource;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        BuildVisual();
        WaterSplashAudio.PlayIceSpearThrow(transform.position);
    }

    private void Update()
    {
        if (hasHit)
        {
            return;
        }

        float stepDistance = speed * Time.deltaTime;
        if (stepDistance <= 0f)
        {
            return;
        }

        if (TryResolveHit(stepDistance))
        {
            return;
        }

        transform.position += (Vector3)(direction * stepDistance);
        traveled += stepDistance;
        if (traveled >= maxRange)
        {
            Shatter();
        }
    }

    private bool TryResolveHit(float stepDistance)
    {
        int collisionMask = groundMask.value | targetMask.value;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, HitRadius, direction, stepDistance, collisionMask);

        bool hasCandidate = false;
        bool candidateIsCombatTarget = false;
        float candidateDistance = float.MaxValue;
        IWaterReactive candidateReactive = null;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            GameObject targetObject = hit.collider.attachedRigidbody != null
                ? hit.collider.attachedRigidbody.gameObject
                : hit.collider.gameObject;

            if (targetObject == source)
            {
                continue;
            }

            bool isCombatTarget = TryGetCombatReactive(targetObject, out IWaterReactive reactive);
            bool isSolidGround = !hit.collider.isTrigger && IsInLayerMask(hit.collider.gameObject.layer, groundMask);
            if (!isCombatTarget && !isSolidGround)
            {
                continue;
            }

            if (hit.distance < candidateDistance)
            {
                hasCandidate = true;
                candidateIsCombatTarget = isCombatTarget;
                candidateReactive = reactive;
                candidateDistance = hit.distance;
            }
        }

        if (!hasCandidate)
        {
            return false;
        }

        Vector2 hitPosition = (Vector2)transform.position + direction * candidateDistance;
        transform.position = new Vector3(hitPosition.x, hitPosition.y, transform.position.z);

        if (candidateIsCombatTarget && candidateReactive != null)
        {
            WaterBurstData burst = new(hitPosition, direction, damage, force, source);
            candidateReactive.ReactToWaterBurst(burst);
            GameAudioController.Play(AudioCue.BurstHit);
            SimpleCameraFollow.RequestHitstop(0.045f);
            SimpleCameraFollow.RequestShake(0.16f, 0.2f);
        }

        Shatter();
        return true;
    }

    private static bool TryGetCombatReactive(GameObject targetObject, out IWaterReactive reactive)
    {
        reactive = null;
        if (targetObject == null)
        {
            return false;
        }

        if (targetObject.TryGetComponent(out EnemyPatrol enemy))
        {
            reactive = enemy;
            return true;
        }

        if (targetObject.TryGetComponent(out BossController boss))
        {
            reactive = boss;
            return true;
        }

        return false;
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void BuildVisual()
    {
        CreateSegment("Trail", new Vector2(-0.46f, 0f), new Vector2(0.82f, 0.12f), TrailColor, 14);
        CreateSegment("Shaft", new Vector2(-0.02f, 0f), new Vector2(0.92f, 0.09f), ShaftColor, 16);
        CreateSegment("Core", new Vector2(0.08f, 0f), new Vector2(0.56f, 0.045f), CoreColor, 17);
        CreateSegment("Tip", new Vector2(0.58f, 0f), new Vector2(0.34f, 0.15f), CoreColor, 18);
    }

    private void CreateSegment(string objectName, Vector2 localPosition, Vector2 localScale, Color color, int sortingOrder)
    {
        GameObject segment = new(objectName);
        segment.transform.SetParent(transform, false);
        segment.transform.localPosition = localPosition;
        segment.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);

        SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
        renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private void Shatter()
    {
        if (hasHit)
        {
            return;
        }

        hasHit = true;
        WaterSplashAudio.PlayIceShatter(transform.position);
        SpawnShards(transform.position, direction);
        Destroy(gameObject);
    }

    private static void SpawnShards(Vector3 position, Vector2 impactDirection)
    {
        Vector2 recoil = impactDirection.sqrMagnitude > 0.01f ? -impactDirection.normalized : Vector2.left;
        for (int i = 0; i < 7; i++)
        {
            GameObject shard = new("IceSpearShard");
            shard.transform.position = new Vector3(position.x, position.y, -0.5f);
            shard.transform.localScale = new Vector3(Random.Range(0.08f, 0.2f), Random.Range(0.035f, 0.075f), 1f);

            SpriteRenderer renderer = shard.AddComponent<SpriteRenderer>();
            renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
            renderer.color = new Color(0.76f, 0.97f, 1f, 0.82f);
            renderer.sortingOrder = 17;

            Vector2 scatter = (recoil + Random.insideUnitCircle * 0.9f).normalized * Random.Range(1.6f, 4.4f);
            IceSpearShard shardMotion = shard.AddComponent<IceSpearShard>();
            shardMotion.Initialize(scatter, Random.Range(-220f, 220f), Random.Range(0.18f, 0.34f));
        }
    }
}

public class IceSpearShard : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector2 velocity;
    private float angularVelocity;
    private float lifetime;
    private float elapsed;

    public void Initialize(Vector2 shardVelocity, float shardAngularVelocity, float shardLifetime)
    {
        velocity = shardVelocity;
        angularVelocity = shardAngularVelocity;
        lifetime = shardLifetime;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.82f, 0f, Mathf.Clamp01(elapsed / lifetime));
            spriteRenderer.color = color;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
