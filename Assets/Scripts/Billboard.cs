using UnityEngine;

public class Billboard : MonoBehaviour
{
    new Transform camera;

    void Start()
    {
        this.camera = Camera.main.transform;
    }

    void Update()
    {
        transform.LookAt(camera);

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }
}
