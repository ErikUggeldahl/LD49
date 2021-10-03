using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    [SerializeField]
    new Camera camera;

    [SerializeField]
    DisasterCoordinator disasters;

    [SerializeField]
    GameObject personPrefab;

    [Header("Parent Objects")]
    [SerializeField]
    Transform buildingPiecesParent;

    [SerializeField]
    Transform previewPiecesParent;

    [SerializeField]
    Transform peopleParent;

    [SerializeField]
    Transform bricksParent;

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

    const float MINOR_PUSH_FORCE = 5000f;
    const float MASSIVE_PUSH_FORCE = 35000f;

    int turnCount = 0;
    int highestTower = 0;

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

    void HandleRestart()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SceneManager.LoadScene("Game");
        }
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

    void HandleDestroyAllBricks()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            foreach (Transform child in bricksParent)
            {
                Destroy(child.gameObject);
            }
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

        var highestYInt = Mathf.CeilToInt(highestY);
        heightDisplay.text = highestYInt.ToString() + "m";

        if (highestYInt > highestTower)
        {
            var difference = highestYInt - highestTower;
            highestTower = highestYInt;

            for (int i = 0; i < difference; i++)
            {
                var randomOnCircle = Random.Range(0, Mathf.PI * 2f);
                var position = new Vector3(Mathf.Cos(randomOnCircle), 0f, Mathf.Sin(randomOnCircle)) * 200f;
                var person = Instantiate(personPrefab, position, Quaternion.identity);
                person.transform.parent = peopleParent;
            }
        }
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
            AwakenBuildingPieces();
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

        foreach (var thunderCloud in FindObjectsOfType<ThunderCloud>())
        {
            thunderCloud.Advance();
        }

        foreach (var piece in buildingPiecesParent.GetComponentsInChildren<BuildingPiece>())
        {
            if (piece.markedForDestruction)
            {
                Destroy(piece.gameObject);
            }
        }

        if (highestTower >= 50 && turnCount % 20 == 0)
        {
            disasters.SpawnThunder();
            if (highestTower >= 125)
            {
                disasters.SpawnThunder();
            }
            if (highestTower >= 200)
            {
                disasters.SpawnThunder();
            }
        }

        if (highestTower >= 100 && turnCount % 50 == 0)
        {
            disasters.StartEarthquake();
        }
    }

    void Update()
    {
        HandleRestart();
        HandleRadialCountAdjust();
        HandleLocalRotationOffset();
        HandleDestroyAllBricks();
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

        if (allPiecesAsleepLastIteration && !Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F) && hit.collider.gameObject.layer == LayerMask.NameToLayer("BuildingPiece"))
        {
            hit.collider.GetComponentInParent<BuildingPiece>().ToggleMarkForDestruction();
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            hit.rigidbody?.AddForce(ray.direction * MINOR_PUSH_FORCE, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            hit.rigidbody?.AddForce(ray.direction * MASSIVE_PUSH_FORCE, ForceMode.Impulse);
        }
    }
}
