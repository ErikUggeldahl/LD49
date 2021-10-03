using UnityEngine;

public class PiecePicker : MonoBehaviour
{
    [SerializeField]
    new Camera camera;

    [SerializeField]
    Builder builder;

    [SerializeField]
    Transform piecesParent;

    bool picking = false;

    PickablePiece selectedPiece;

    public void SelectPiece(PickablePiece piece)
    {
        if (selectedPiece)
        {
            selectedPiece.SetPickState(PickablePiece.PickState.None);
        }
        selectedPiece = piece;
        selectedPiece.SetPickState(PickablePiece.PickState.Picked);

        builder.SetBuildingPiece(piece.piece);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            TogglePicking();
        }
    }

    void TogglePicking()
    {
        picking = !picking;
        camera.enabled = picking;
        builder.AbleToBuild(!picking);
    }
}
