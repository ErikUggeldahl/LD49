using UnityEngine;

public class PickablePiece : MonoBehaviour
{
    public enum PickState
    {
        None,
        Picked,
    }
    PickState state = PickState.None;

    Color colorNone = Color.white;
    Color colorPicked = Color.green;
    Color colorHighlighted = Color.yellow;

    public BuildingPiece piece;
    Renderer[] renderers;

    public void SetPickState(PickState state)
    {
        if (state == PickState.None)
        {
            UpdateColours(colorNone);
            this.state = state;
        }
        else if (state == PickState.Picked)
        {
            UpdateColours(colorPicked);
            this.state = state;
        }
    }

    void UpdateColours(Color color)
    {
        foreach (var renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    void Start()
    {
        piece = GetComponentInParent<BuildingPiece>();
        renderers = piece.GetComponentsInChildren<Renderer>();
    }

    void OnMouseEnter()
    {
        if (state == PickState.None)
        {
            UpdateColours(colorHighlighted);
        }
    }

    void OnMouseExit()
    {
        if (state == PickState.None)
        {
            UpdateColours(colorNone);
        }
    }

    void OnMouseDown()
    {
        GetComponentInParent<PiecePicker>().SelectPiece(this);
    }
}
