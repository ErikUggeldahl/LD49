using UnityEngine;

public class BuildingPiece : MonoBehaviour
{
    public string pieceName;
    public int height = 0;
    public GameObject prefab;
    public GameObject previewPrefab;
    public bool markedForDestruction = false;

    [SerializeField]
    GameObject brickPrefab;

    const float TRIGGER_VELOCITY_MAGNITUDE = 20f;

    public void ToggleMarkForDestruction()
    {
        markedForDestruction = !markedForDestruction;
        Color markColor = markedForDestruction ? Color.black : Color.white;
        GetComponentInChildren<Renderer>().material.color = markColor;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > TRIGGER_VELOCITY_MAGNITUDE)
        {
            Destroy(gameObject);

            for (var i = 0; i < 5; i++)
            {
                var brick = Instantiate(brickPrefab, transform.position, transform.rotation);
                var ejectDirection = collision.relativeVelocity / 2f + Random.insideUnitSphere * collision.relativeVelocity.magnitude / 2f;
                brick.GetComponent<Rigidbody>().AddForce(ejectDirection, ForceMode.VelocityChange);
            }
        }
    }
}
