using UnityEngine;

public class FishBankController : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject fishPrefab;
    public int numberOfFish = 50;

    [Header("Movement Volume (Cube)")]
    public Vector3 volumeSize = new Vector3(5, 3, 5);

    [Header("Swim Behavior")]
    public float swimSpeed = 1.5f;
    public float changeTargetInterval = 3f;
    public float turnSpeed = 2f;
    [Range(0f, 1f)] public float bankInfluence = 0.3f;

    [Header("Rotation Offset (Fix Facing)")]
    public Vector3 rotationOffsetEuler = Vector3.zero;

    private FishUnit[] fishUnits;
    private Vector3 lastBankPosition;

    void Start()
    {
        fishUnits = new FishUnit[numberOfFish];
        lastBankPosition = transform.position;

        for (int i = 0; i < numberOfFish; i++)
        {
            Vector3 localPos = GetRandomPointInVolume();
            GameObject fish = Instantiate(fishPrefab, transform.TransformPoint(localPos), Quaternion.identity, transform);

            Animator animator = fish.GetComponent<Animator>();
            if (animator != null)
                animator.Play(0, 0, Random.Range(0f, 1f));

            fishUnits[i] = new FishUnit
            {
                transform = fish.transform,
                targetLocalPos = GetRandomPointInVolume(),
                changeTimer = Random.Range(0f, changeTargetInterval)
            };
        }
    }

    void Update()
    {
        Vector3 bankVelocity = (transform.position - lastBankPosition) / Time.deltaTime;
        lastBankPosition = transform.position;

        // Recompute offset every frame — NOW works at runtime!
        Quaternion rotationOffset = Quaternion.Euler(rotationOffsetEuler);

        foreach (var fish in fishUnits)
        {
            fish.changeTimer -= Time.deltaTime;
            if (fish.changeTimer <= 0f)
            {
                fish.targetLocalPos = GetRandomPointInVolume();
                fish.changeTimer = Random.Range(1f, changeTargetInterval);
            }

            // Movement
            Vector3 localPos = transform.InverseTransformPoint(fish.transform.position);
            Vector3 localDirection = fish.targetLocalPos - localPos;
            Vector3 newLocalPos = Vector3.MoveTowards(localPos, fish.targetLocalPos, swimSpeed * Time.deltaTime);
            fish.transform.position = transform.TransformPoint(newLocalPos);

            // Rotation
            Vector3 worldLocalDirection = transform.TransformDirection(localDirection.normalized);
            Vector3 totalDirection = Vector3.Lerp(worldLocalDirection, bankVelocity.normalized, bankInfluence).normalized;

            if (totalDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(totalDirection);
                fish.transform.rotation = Quaternion.Slerp(fish.transform.rotation, targetRotation * rotationOffset, Time.deltaTime * turnSpeed);
            }
        }
    }

    Vector3 GetRandomPointInVolume()
    {
        return new Vector3(
            Random.Range(-volumeSize.x / 2f, volumeSize.x / 2f),
            Random.Range(-volumeSize.y / 2f, volumeSize.y / 2f),
            Random.Range(-volumeSize.z / 2f, volumeSize.z / 2f)
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, volumeSize);
    }

    private class FishUnit
    {
        public Transform transform;
        public Vector3 targetLocalPos;
        public float changeTimer;
    }
}
