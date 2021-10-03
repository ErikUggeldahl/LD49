using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    [SerializeField]
    new Camera camera;

    [SerializeField]
    Transform buildingPiecesParent;

    [SerializeField]
    Transform previewPiecesParent;

    [Header("UI Elements")]
    [SerializeField]
    Text symmetryDisplay;

    [SerializeField]
    Text heightDisplay;

    [SerializeField]
    Text turnDisplay;

    [SerializeField]
    GameObject awaitingDisplay;

    bool ableToBuild = true;
    BuildingPiece pieceToBuild;

    int turnCount = 0;

    const int MAX_RADIAL = 24;
    int radialCount = 4;

    const float LOCAL_ROTATION_DEG_PER_SEC = 90f;
    const float LOCAL_ROTATION_MOUSE_FACTOR = 3f;
    float localRotationOffset = 0f;

    int RAYCAST_MASK;

    bool allPiecesAsleepLastIteration = true;

    List<GameObject> previewObjects = new List<GameObject>(MAX_RADIAL);

    public void AwakenBuildingPieces()
    {
        allPiecesAsleepLastIteration = false;

        foreach (var rigidbody in buildingPiecesParent.GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.WakeUp();
        }
    }

    public void SetBuildingPiece(BuildingPiece piece)
    {
        foreach (var obj in previewObjects)
        {
            Destroy(obj);
        }
        previewObjects.Clear();

        pieceToBuild = piece;

        for (int i = 0; i < MAX_RADIAL; i++)
        {
            var preview = Instantiate(piece.previewPrefab, previewPiecesParent);
            preview.name = piece.previewPrefab.name + " " + i;

            previewObjects.Add(preview);
        }

        SetRadialCount(radialCount);
    }

    public void AbleToBuild(bool able)
    {
        ableToBuild = able;
    }

    void Start()
    {
        RAYCAST_MASK = LayerMask.GetMask("Ground", "BuildingPiece");

        SetRadialCount(radialCount);
    }

    void SetRadialCount(int radialCount)
    {
        this.radialCount = radialCount;
        for (var i = 0; i < previewObjects.Count; i++)
        {
            previewObjects[i].SetActive(i < radialCount);
        }
        symmetryDisplay.text = radialCount.ToString();
    }

    void HandleRadialCountAdjust()
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

    void CalculateScore()
    {
        var highestY = 0f;
        foreach (Transform transform in buildingPiecesParent)
        {
            var buildingPiece = transform.GetComponent<BuildingPiece>();
            var yExtent = transform.position.y + buildingPiece.height;
            if (yExtent > highestY)
            {
                highestY = yExtent;
            }
        }
        heightDisplay.text = Mathf.CeilToInt(highestY).ToString() + "m";
    }

    void AssessBuildingPieceActivity()
    {
        if (allPiecesAsleepLastIteration) return;

        var allAsleep = true;
        foreach (var rigidbody in buildingPiecesParent.GetComponentsInChildren<Rigidbody>())
        {
            if (rigidbody.IsSleeping())
            {
                rigidbody.GetComponentInChildren<Renderer>().material.color = Color.white;
            }
            else
            {
                allAsleep = false;
                rigidbody.GetComponentInChildren<Renderer>().material.color = Color.red;
            }
        }
        awaitingDisplay.SetActive(!allAsleep);
        allPiecesAsleepLastIteration = allAsleep;
    }

    void HandleTurnSkip()
    {
        if (allPiecesAsleepLastIteration && Input.GetKeyDown(KeyCode.Z))
        {
            AdvanceTurn();
        }
    }

    void AdvanceTurn()
    {
        turnCount++;
        turnDisplay.text = turnCount.ToString();

        foreach (var constructor in buildingPiecesParent.GetComponentsInChildren<BuildingConstructor>())
        {
            constructor.AdvanceLayer();
        }
    }

    void Update()
    {
        HandleRadialCountAdjust();
        HandleLocalRotationOffset();
        CalculateScore();
        AssessBuildingPieceActivity();
        HandleTurnSkip();

        if (pieceToBuild == null || !ableToBuild) return;

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

        if (allPiecesAsleepLastIteration && Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < radialCount; i++)
            {
                Instantiate(pieceToBuild.prefab, previewObjects[i].transform.position, previewObjects[i].transform.rotation, buildingPiecesParent);
            }

            allPiecesAsleepLastIteration = false;

            AdvanceTurn();
        }
    }
}
