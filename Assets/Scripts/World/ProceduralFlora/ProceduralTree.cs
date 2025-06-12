using UnityEngine;

public class ProceduralTree : MonoBehaviour
{
    [Header("Tree Settings")]
    public int trunkHeight = 4;
    public int crownRadius = 2;

    [Header("Sphere Settings")]
    public GameObject spherePrefab;
    public Material dirtMaterial;
    public Material grassMaterial;

    [Header("Scale Variation")]
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    void Start()
    {
        GenerateTree();
    }

    void GenerateTree()
    {
        if (!spherePrefab || !dirtMaterial || !grassMaterial) return;

        // 🪵 Generate a thin vertical trunk (1 sphere wide)
        for (int y = 0; y < trunkHeight; y++)
        {
            Vector3 pos = transform.position + new Vector3(0, y, 0);
            GameObject trunkSphere = Instantiate(spherePrefab, pos, Quaternion.identity, transform);
            trunkSphere.GetComponent<Renderer>().material = dirtMaterial;
            trunkSphere.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
        }

        // 🍃 Generate rounded crown of leaves at top
        Vector3 crownCenter = transform.position + new Vector3(0, trunkHeight, 0);
        for (int x = -crownRadius; x <= crownRadius; x++)
        {
            for (int y = -crownRadius; y <= crownRadius; y++)
            {
                for (int z = -crownRadius; z <= crownRadius; z++)
                {
                    Vector3 offset = new Vector3(x, y, z);
                    if (offset.magnitude <= crownRadius)
                    {
                        Vector3 pos = crownCenter + offset;
                        GameObject leafSphere = Instantiate(spherePrefab, pos, Quaternion.identity, transform);
                        leafSphere.GetComponent<Renderer>().material = grassMaterial;
                        leafSphere.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
                    }
                }
            }
        }
    }
}