using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class BackgroundFlier : MonoBehaviour, IOnPool
{
    [field: SerializeField]
    [field: Tooltip("The direction the sprite is facing. Used to point the sprite in the correct direction")]
    float spriteDirection;

    [field: SerializeField]
    [field: Tooltip("Specifies the min and max velocity the flier can travel at")]
    [field: FormerlySerializedAs("velocityRange")]
    public Vector2 VelocityRange = new Vector2(10f, 15f);

    [field: SerializeField]
    [field: Tooltip("The maximum lifetime of the object, which is randomly chosen between the two ranges")]
    Vector2 maxLifetimeRange = new Vector2(10f,10f);

    [field: SerializeField]
    [field: Tooltip("The maximum travel distance of the object, which is randomly chosen between the two ranges")]
    Vector2 maxTravelDistanceRange = new Vector2(50f,50f);

    [field: SerializeField]
    [field: Tooltip("Specifies the min and max directional variance")]
    Vector2 directionalVariance = new Vector2(-5f, 5f);

    [field: SerializeField]
    [field: Tooltip("What to do when this object is done")]
    public OnDoneBehaviour DoneBehaviour { get; private set; } = OnDoneBehaviour.DestroyOrPool;

    public Vector3 TravelVector { get; private set; }
    public Quaternion TravelDirection { get; private set; }
    public float TravelVelocity { get; private set; }
    public float LifeTime { get; private set; }
    public float MaxTravelDistance { get; private set; }
    public float DirectionalVariance { get; private set; }

    public float SpawnTime { get; private set; }

    public void OnPool()
    {
        TravelVector = default;
        TravelDirection = Quaternion.identity;
        TravelVelocity = default;
        LifeTime = 0;
        MaxTravelDistance = default;
        DirectionalVariance = default;
        SpawnTime = default;
    }

    private void Start()
    {
        DirectionalVariance = directionalVariance.RandomInRange();
        transform.rotation *= Quaternion.Euler(0, 0, DirectionalVariance);
        TravelDirection = transform.rotation;
        TravelVelocity = VelocityRange.RandomInRange();
        transform.rotation *= Quaternion.Euler(0,0,-spriteDirection);
        LifeTime = maxLifetimeRange.RandomInRange();
        TravelVector = TravelDirection * (Vector3.right * TravelVelocity);
        MaxTravelDistance = maxTravelDistanceRange.RandomInRange();
        SpawnTime = Time.time;
    }

    private void Update()
    {
        transform.position += TravelVector * Time.deltaTime;
        if (Time.time >= SpawnTime + LifeTime)
        {
            DoneBehaviour.DoneWithObject(this);
        }
    }
}
