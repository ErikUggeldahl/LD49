using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [SerializeField]
    new Camera camera;

    [SerializeField]
    GameObject wallPrefab;

    [SerializeField]
    Material previewMaterial;

    const int MAX_RADIAL = 24;
    int radialCount = 4;

    const float LOCAL_ROTATION_DEG_PER_SEC = 90f;
    const float LOCAL_ROTATION_MOUSE_FACTOR = 3f;
    float localRotationOffset = 0f;

    int LAYER_GROUND;
    int LAYER_BUILDING_PIECE;
    int LAYER_PREVIEW;
    int RAYCAST_MASK;

    List<GameObject> previewObjects = new List<GameObject>(MAX_RADIAL);

    void Start()
    {
        LAYER_GROUND = LayerMask.NameToLayer("Ground");
        LAYER_BUILDING_PIECE = LayerMask.NameToLayer("BuildingPiece");
        LAYER_PREVIEW = LayerMask.NameToLayer("Preview");

        RAYCAST_MASK = LayerMask.GetMask("Ground", "BuildingPiece");

        for (int i = 0; i < MAX_RADIAL; i++)
        {
            var preview = Instantiate(wallPrefab);
            preview.name = "Preview " + wallPrefab.name + " " + i;
            foreach (var child in preview.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = LAYER_PREVIEW;
            }
            foreach (var rb in preview.GetComponentsInChildren<Rigidbody>())
            {
                Destroy(rb);
            }
            foreach (var collider in preview.GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
            }
            foreach (var renderer in preview.GetComponentsInChildren<Renderer>())
            {
                renderer.material = previewMaterial;
            }

            previewObjects.Add(preview);
        }

        SetRadialCount(radialCount);
    }

    void SetRadialCount(int radialCount)
    {
        this.radialCount = radialCount;
        for (var i = 0; i < MAX_RADIAL; i++)
        {
            previewObjects[i].SetActive(i < radialCount);
        }
    }

    void HandleRadialAdjust()
    {
        var scroll = Mathf.Clamp(Input.mouseScrollDelta.y, -1.0f, 1.0f);
        var newRadialCount = Mathf.Clamp(radialCount + (int)scroll, 1, MAX_RADIAL);
        if (newRadialCount != radialCount)
        {
            SetRadialCount(newRadialCount);
        }
    }

    void HandleLocalRotationOffset()
    {
        var localRotationDelta = 0f;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.Q))
            {
                localRotationDelta -= 1f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                localRotationDelta += 1f;
            }
            localRotationDelta += Input.GetAxis("Mouse X") * LOCAL_ROTATION_MOUSE_FACTOR;
            localRotationOffset += localRotationDelta * LOCAL_ROTATION_DEG_PER_SEC * Time.deltaTime;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                localRotationDelta -= 45f;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                localRotationDelta += 45f;
            }
            localRotationOffset += localRotationDelta;
        }

        if (Input.GetKey(KeyCode.R))
        {
            localRotationOffset = 0f;
        }
    }

    void Update()
    {
        HandleRadialAdjust();
        HandleLocalRotationOffset();

        RaycastHit hit;
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        var isHit = Physics.Raycast(ray, out hit, 1000f, RAYCAST_MASK);

        if (!isHit) return;

        var floorPosition = Input.GetKey(KeyCode.LeftControl) ? previewObjects[0].transform.position : hit.point;
        floorPosition.y = 0;

        for (var i = 0; i < radialCount; i++)
        {
            var degrees = 360f / radialCount;
            var position = Quaternion.Euler(0f, degrees * i, 0f) * floorPosition;
            var rotation = Quaternion.LookRotation(position, Vector3.up);
            rotation *= Quaternion.Euler(0f, localRotationOffset, 0f);

            if (!Input.GetKey(KeyCode.LeftControl))
            {
                position.y = hit.point.y;
                previewObjects[i].transform.position = position;
            }
            previewObjects[i].transform.rotation = rotation;
        }

        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < radialCount; i++)
            {
                var placedObject = Instantiate(wallPrefab, previewObjects[i].transform.position, previewObjects[i].transform.rotation);
                placedObject.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}
