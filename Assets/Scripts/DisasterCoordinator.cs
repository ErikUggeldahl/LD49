using System.Collections;
using UnityEngine;

public class DisasterCoordinator : MonoBehaviour
{
    [SerializeField]
    Builder builder;

    [Header("Earthquake")]
    [SerializeField]
    Transform ground;

    [SerializeField]
    float earthquakeAmplitude;

    [SerializeField]
    float earthquakeDuration;

    [SerializeField]
    float earthquakePeriod;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            StartEarthquake();
        }
    }

    void StartEarthquake()
    {
        StartCoroutine(Earthquake());
    }

    IEnumerator Earthquake()
    {
        builder.AwakenBuildingPieces();

        var originalPosition = ground.position;
        var highPosition = ground.position + new Vector3(0f, earthquakeAmplitude, 0f);
        var lowPosition = ground.position + new Vector3(0f, -earthquakeAmplitude, 0f);
        var time = 0f;
        while (time < earthquakeDuration)
        {
            time += Time.fixedDeltaTime;
            var lerpT = Mathf.Sin(time * Mathf.PI * 2f / earthquakePeriod) / 2f + 0.5f;
            ground.position = Vector3.Lerp(highPosition, lowPosition, lerpT);

            yield return new WaitForFixedUpdate();
        }

        time = 0;
        var endOfQuakePosition = ground.position;
        while (time < 1f)
        {
            time += Time.fixedDeltaTime;
            ground.position = Vector3.Lerp(endOfQuakePosition, originalPosition, time);

            yield return new WaitForFixedUpdate();
        }

        ground.position = originalPosition;
    }
}
