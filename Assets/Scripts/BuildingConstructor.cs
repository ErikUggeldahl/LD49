using UnityEngine;

public class BuildingConstructor : MonoBehaviour
{
    [SerializeField]
    Transform scaler;

    [SerializeField]
    Transform scaffolding;

    int layerCount;
    int currentLayer = 0;
    Transform[] scaffoldingLayers;

    void Start()
    {
        layerCount = scaffolding.childCount;
        scaffoldingLayers = new Transform[layerCount];
        for (var i = 0; i < layerCount; i++)
        {
            scaffoldingLayers[i] = scaffolding.GetChild(i);
            scaffoldingLayers[i].gameObject.SetActive(false);
        }

        AdvanceLayer();
    }

    void CompleteBuild()
    {
        scaler.GetChild(0).SetParent(scaler.parent);
        Destroy(scaler.gameObject);
        Destroy(scaffolding.gameObject);
        Destroy(this);
    }

    public void AdvanceLayer()
    {
        // Small workaround: this method is called before Start in the spawning method in Builder.
        // Ignore that first call and call it ourselves after Start.
        if (layerCount == 0) return;

        if (currentLayer == layerCount)
        {
            CompleteBuild();
            return;
        }

        scaffoldingLayers[currentLayer].gameObject.SetActive(true);

        currentLayer++;

        scaler.localScale = new Vector3(1f, (float)currentLayer / layerCount, 1f);
    }
}
