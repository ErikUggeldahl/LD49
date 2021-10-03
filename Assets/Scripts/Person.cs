using System.Collections;
using UnityEngine;

public class Person : MonoBehaviour
{
    float idealDistance;

    void Start()
    {
        idealDistance = Random.Range(50f, 100f);
        transform.LookAt(Vector3.zero);

        StartCoroutine(Pilgrimage());
    }

    IEnumerator Pilgrimage()
    {
        while (transform.position.magnitude > idealDistance)
        {
            transform.position += transform.forward * Time.deltaTime * 2f;
            yield return null;
        }
    }
}
