using System.Collections;
using UnityEngine;

public class ThunderCloud : MonoBehaviour
{
    [SerializeField]
    float timeToTarget = 5f;

    [SerializeField]
    float distanceFromTarget = 50f;

    [SerializeField]
    float additionalHeightOnTarget = 40f;

    [SerializeField]
    LineRenderer lightningBolt;

    const float LIGHTNING_FORCE = 25000f;

    Ray target;
    bool advanced = false;

    public void Advance()
    {
        advanced = true;
    }

    void Start()
    {
        var allPieces = GameObject.FindGameObjectsWithTag("BuildingPiece");
        if (allPieces.Length == 0)
        {
            Destroy(gameObject);
            return;
        }
        var targetPiece = allPieces[Random.Range(0, allPieces.Length)].transform;

        var position = targetPiece.position + targetPiece.position.normalized * distanceFromTarget + new Vector3(0f, additionalHeightOnTarget, 0f);
        var direction = (targetPiece.position - position).normalized;
        Debug.DrawRay(position, direction, Color.red, 20f);
        target = new Ray(position, direction);

        StartCoroutine(GoToPosition());
    }

    IEnumerator GoToPosition()
    {
        while (!advanced)
        {
            yield return null;
        }

        var t = 0f;
        Vector3 originalPosition = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / timeToTarget;
            transform.position = Vector3.Lerp(originalPosition, target.origin, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        RaycastHit hit;
        var isHit = Physics.Raycast(target, out hit, 1000f, LayerMask.GetMask("BuildingPiece"));
        if (!isHit) yield break;

        advanced = false;
        while (!advanced)
        {
            yield return null;
        }

        hit.collider.attachedRigidbody.AddForce(target.direction * LIGHTNING_FORCE, ForceMode.Impulse);

        InitializeLightningBolt(hit.point);

        var mainLight = FindObjectOfType<Light>();
        mainLight.intensity = 10f;
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        mainLight.intensity = 1f;
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        mainLight.intensity = 10f;
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        mainLight.intensity = 1f;
        yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        Destroy(gameObject);
    }

    void InitializeLightningBolt(Vector3 end)
    {
        lightningBolt.enabled = true;
        Vector3[] vertexPositions = new Vector3[lightningBolt.positionCount];
        Vector3 localEnd = transform.InverseTransformPoint(end);
        for (var i = 0; i < lightningBolt.positionCount - 1; i++)
        {
            vertexPositions[i] = Vector3.Lerp(Vector3.zero, localEnd, (float)i / lightningBolt.positionCount) + Random.insideUnitSphere * 5f;
        }
        lightningBolt.SetPositions(vertexPositions);
        lightningBolt.SetPosition(lightningBolt.positionCount - 1, localEnd);
    }
}
