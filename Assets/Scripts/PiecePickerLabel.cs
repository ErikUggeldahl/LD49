using UnityEngine;
using UnityEngine.UI;

public class PiecePickerLabel : MonoBehaviour
{
    void Start()
    {
        var building = GetComponentInParent<BuildingPiece>();
        GetComponentInChildren<Text>().text = building.pieceName;
    }
}
