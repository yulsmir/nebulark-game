using UnityEngine;

public class ProceduralFlower : MonoBehaviour
{
    [Header("Settings")]
    public int stemHeight = 3;
    public int petalCount = 6;
    public float petalRadius = 0.4f;

    [Header("Prefabs & Materials")]
    public GameObject spherePrefab;
    public Material stemMaterial;
    public Material petalMaterial;

    [Header("Scale Variation")]
    public float minScale = 0.8f;
    public float maxScale = 1.1f;

    void Start()
    {
        GenerateFlower();
    }

    void GenerateFlower()
    {
        if (!spherePrefab || !stemMaterial || !petalMaterial) return;

        // 🌱 Stem
        for (int y = 0; y < stemHeight; y++)
        {
            Vector3 pos = transform.position + new Vector3(0, y, 0);
            GameObject stemPart = Instantiate(spherePrefab, pos, Quaternion.identity, transform);
            stemPart.GetComponent<Renderer>().material = stemMaterial;
            stemPart.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
        }

        // 🌸 Petals
        Vector3 center = transform.position + new Vector3(0, stemHeight, 0);
        for (int i = 0; i < petalCount; i++)
        {
            float angle = i * Mathf.PI * 2f / petalCount;
            float x = Mathf.Cos(angle) * petalRadius;
            float z = Mathf.Sin(angle) * petalRadius;
            Vector3 offset = new Vector3(x, 0, z);

            GameObject petal = Instantiate(spherePrefab, center + offset, Quaternion.identity, transform);
            petal.GetComponent<Renderer>().material = petalMaterial;
            petal.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
        }
    }
}