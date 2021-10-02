using UnityEngine;

public class Spinner : MonoBehaviour
{
    RectTransform rect;
    const float ROTATION_DEG_PER_SEC = -90f;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        rect.Rotate(Vector3.forward, ROTATION_DEG_PER_SEC * Time.deltaTime);
    }
}
