
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Noise Settings")]
    public float scale = 0.1f;
    public float heightMultiplier = 8f;
    public int chunkRadius = 8;

    [Header("Sphere Prefab")]
    public GameObject spherePrefab;

    [Header("Materials")]
    public Material grassMaterial;
    public Material dirtMaterial;
    public Material stoneMaterial;
    public Material waterMaterial;
    public Material sandMaterial;
    public Material snowMaterial;

    [Header("Sphere Size Settings")]
    public float minSphereScale = 0.8f;
    public float maxSphereScale = 1.2f;

    [Header("Player Reference")]
    public Transform player;

    private HashSet<Vector3Int> generatedPositions = new HashSet<Vector3Int>();

    void Update()
    {
        if (player == null || spherePrefab == null) return;

        GenerateTerrainAroundPlayer();
    }

    void GenerateTerrainAroundPlayer()
    {
        Vector3Int playerPos = Vector3Int.RoundToInt(player.position);

        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int z = -chunkRadius; z <= chunkRadius; z++)
            {
                int worldX = playerPos.x + x;
                int worldZ = playerPos.z + z;

                float yNoise = Mathf.PerlinNoise(worldX * scale, worldZ * scale) * heightMultiplier;
                int yMax = Mathf.Max(1, Mathf.FloorToInt(yNoise));

                for (int y = 0; y <= yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(worldX, y, worldZ);
                    if (generatedPositions.Contains(pos)) continue;

                    GameObject block = Instantiate(spherePrefab, pos, Quaternion.identity, transform);

                    float randomScale = Random.Range(minSphereScale, maxSphereScale);
                    block.transform.localScale = Vector3.one * randomScale;

                    Renderer renderer = block.GetComponent<Renderer>();
                    SphereBlock blockData = block.GetComponent<SphereBlock>();
                    if (blockData != null) blockData.gridPosition = pos;

                    if (y <= 2)
                    {
                        if (blockData != null) blockData.type = SphereType.Water;
                        renderer.material = waterMaterial;
                    }
                    else if (y < 4)
                    {
                        if (blockData != null) blockData.type = SphereType.Sand;
                        renderer.material = sandMaterial;
                    }
                    else if (y < 6)
                    {
                        if (blockData != null) blockData.type = SphereType.Dirt;
                        renderer.material = dirtMaterial;
                    }
                    else if (y < 8)
                    {
                        if (blockData != null) blockData.type = SphereType.Stone;
                        renderer.material = stoneMaterial;
                    }
                    else if (y > 8)
                    {
                        if (blockData != null) blockData.type = SphereType.Snow;
                        renderer.material = snowMaterial;
                    }
                    else
                    {
                        if (blockData != null) blockData.type = SphereType.Grass;
                        renderer.material = grassMaterial;
                    }

                    if (block.GetComponent<Collider>() == null)
                        block.AddComponent<SphereCollider>();

                    block.isStatic = true;
                    block.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
                    block.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    generatedPositions.Add(pos);
                }
            }
        }
    }
}
