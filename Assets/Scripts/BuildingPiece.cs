using UnityEngine;

public class BuildingPiece : MonoBehaviour
{
    public string pieceName;
    public int height = 0;
    public GameObject prefab;
    public GameObject previewPrefab;
    public bool markedForDestruction = false;

    public void ToggleMarkForDestruction()
    {
        markedForDestruction = !markedForDestruction;
        Color markColor = markedForDestruction ? Color.black : Color.white;
        GetComponentInChildren<Renderer>().material.color = markColor;
    }
}
