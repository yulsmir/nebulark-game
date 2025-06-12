using System.Collections.Generic;
using UnityEngine;
using World;

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

    [Header("Unloading Settings")]
    public int unloadRadius = 12;

    [Header("Procedural Content")]
    public GameObject treePrefab;
    public GameObject flowerPrefab;
    public GameObject creaturePrefab;

    private Dictionary<Vector3, GameObject> spawnedBlocks = new();
    private Dictionary<Vector3, GameObject> spawnedFlora = new();
    private HashSet<Vector3> generatedPositions = new();

    void Update()
    {
        if (player == null || spherePrefab == null) return;

        GenerateTerrainAroundPlayer();
        UnloadDistantTerrain();
        UnloadDistantFlora();
    }

    void GenerateTerrainAroundPlayer()
    {
        Vector3 playerPos = player.position;

        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int z = -chunkRadius; z <= chunkRadius; z++)
            {
                int worldX = Mathf.FloorToInt(playerPos.x) + x;
                int worldZ = Mathf.FloorToInt(playerPos.z) + z;

                float yNoise = Mathf.PerlinNoise(worldX * scale, worldZ * scale) * heightMultiplier;
                int yMax = Mathf.Max(1, Mathf.FloorToInt(yNoise));

                for (int y = 0; y <= yMax; y++)
                {
                    float scaleNoise = Mathf.PerlinNoise((worldX + 100) * 0.2f, (worldZ + 100) * 0.2f);
                    float scale = Mathf.Lerp(minSphereScale, maxSphereScale, scaleNoise);

                    Vector3 pos = new Vector3(worldX * scale, y * scale, worldZ * scale);
                    Vector3 key = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

                    if (generatedPositions.Contains(key)) continue;

                    GameObject block = Instantiate(spherePrefab, pos, Quaternion.identity, transform);
                    block.transform.localScale = Vector3.one * scale;

                    Renderer renderer = block.GetComponent<Renderer>();
                    SphereBlock blockData = block.GetComponent<SphereBlock>();
                    if (blockData != null) blockData.gridPosition = key;

                    if (y <= 2)       { if (blockData != null) blockData.type = SphereType.Water;  renderer.material = waterMaterial; }
                    else if (y < 4)   { if (blockData != null) blockData.type = SphereType.Sand;   renderer.material = sandMaterial; }
                    else if (y < 6)   { if (blockData != null) blockData.type = SphereType.Dirt;   renderer.material = dirtMaterial; }
                    else if (y < 8)   { if (blockData != null) blockData.type = SphereType.Stone;  renderer.material = stoneMaterial; }
                    else if (y > 8)   { if (blockData != null) blockData.type = SphereType.Snow;   renderer.material = snowMaterial; }
                    else             { if (blockData != null) blockData.type = SphereType.Grass;  renderer.material = grassMaterial; }

                    if (block.GetComponent<Collider>() == null)
                        block.AddComponent<SphereCollider>();

                    block.isStatic = true;
                    block.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
                    block.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    generatedPositions.Add(key);
                    spawnedBlocks[key] = block;

                    // Flora generation
                    if (y == yMax)
                    {
                        float chance = Random.value;

                        if (blockData.type == SphereType.Grass)
                        {
                            if (chance < 0.05f && treePrefab && !spawnedFlora.ContainsKey(key))
                                spawnedFlora[key] = Instantiate(treePrefab, pos + Vector3.up * 0.5f, Quaternion.identity, transform);
                            else if (chance < 0.10f && flowerPrefab && !spawnedFlora.ContainsKey(key))
                                spawnedFlora[key] = Instantiate(flowerPrefab, pos + Vector3.up * 0.3f, Quaternion.identity, transform);
                        }
                        else if (blockData.type == SphereType.Sand)
                        {
                            if (chance < 0.03f && creaturePrefab && !spawnedFlora.ContainsKey(key))
                                spawnedFlora[key] = Instantiate(creaturePrefab, pos + Vector3.up * 0.5f, Quaternion.identity, transform);
                        }
                    }
                }
            }
        }
    }

    void UnloadDistantTerrain()
    {
        Vector3 playerPos = player.position;
        List<Vector3> toRemove = new();

        foreach (var pair in spawnedBlocks)
        {
            if (Vector3.Distance(pair.Key, playerPos) > unloadRadius)
            {
                Destroy(pair.Value);
                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
        {
            spawnedBlocks.Remove(key);
            generatedPositions.Remove(key);
        }
    }

    void UnloadDistantFlora()
    {
        Vector3 playerPos = player.position;
        List<Vector3> toRemove = new();

        foreach (var pair in spawnedFlora)
        {
            if (Vector3.Distance(pair.Key, playerPos) > unloadRadius)
            {
                Destroy(pair.Value);
                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
            spawnedFlora.Remove(key);
    }
}
