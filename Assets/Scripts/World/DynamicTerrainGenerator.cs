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

    [Header("Pooling")]
    public ObjectPooler terrainPool;
    public ObjectPooler treePool;
    public ObjectPooler flowerPool;
    public ObjectPooler creaturePool;

    private Dictionary<Vector3, GameObject> spawnedBlocks = new();
    private Dictionary<Vector3, GameObject> spawnedFlora = new();
    private HashSet<Vector3> generatedPositions = new();

    void Update()
    {
        if (player == null || terrainPool == null) return;

        GenerateTerrainAroundPlayer();
        UnloadDistantTerrain();
        UnloadDistantFlora();
    }

    void GenerateTerrainAroundPlayer()
    {
        Vector3 playerPos = player.position;
        float averageScale = (minSphereScale + maxSphereScale) / 2f;
        float spacing = averageScale * 0.9f;

        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int z = -chunkRadius; z <= chunkRadius; z++)
            {
                int worldX = Mathf.FloorToInt(playerPos.x / spacing) + x;
                int worldZ = Mathf.FloorToInt(playerPos.z / spacing) + z;

                float yNoise = Mathf.PerlinNoise(worldX * scale, worldZ * scale) * heightMultiplier;
                int yMax = Mathf.Max(1, Mathf.FloorToInt(yNoise));

                for (int y = 0; y <= yMax; y++)
                {
                    Vector3 pos = new Vector3(worldX * spacing, y * spacing, worldZ * spacing);
                    Vector3 key = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

                    if (generatedPositions.Contains(key)) continue;

                    GameObject block = terrainPool.Get(pos, Quaternion.identity, transform);
                    block.transform.localScale = Vector3.one * spacing;

                    Renderer renderer = block.GetComponent<Renderer>();
                    SphereBlock blockData = block.GetComponent<SphereBlock>();
                    if (blockData != null) blockData.gridPosition = Vector3Int.RoundToInt(key);

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

                        if (blockData.type == SphereType.Grass && !spawnedFlora.ContainsKey(key))
                        {
                            if (chance < 0.05f && treePool)
                                spawnedFlora[key] = treePool.Get(pos + Vector3.up * 0.5f, Quaternion.identity, transform);
                            else if (chance < 0.10f && flowerPool)
                                spawnedFlora[key] = flowerPool.Get(pos + Vector3.up * 0.3f, Quaternion.identity, transform);
                        }
                        else if (blockData.type == SphereType.Sand && chance < 0.03f && creaturePool && !spawnedFlora.ContainsKey(key))
                        {
                            spawnedFlora[key] = creaturePool.Get(pos + Vector3.up * 0.5f, Quaternion.identity, transform);
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
                terrainPool.Return(pair.Value);
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
                if (pair.Value.name.Contains("Tree"))      treePool?.Return(pair.Value);
                else if (pair.Value.name.Contains("Flower")) flowerPool?.Return(pair.Value);
                else if (pair.Value.name.Contains("Creature")) creaturePool?.Return(pair.Value);
                else Destroy(pair.Value);

                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
            spawnedFlora.Remove(key);
    }
}
