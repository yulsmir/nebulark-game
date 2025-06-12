
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

    [Header("Unloading Settings")]
    public int unloadRadius = 12;

    [Header("Procedural Content")]
    public GameObject treePrefab;
    public GameObject flowerPrefab;
    public GameObject creaturePrefab;

    private Dictionary<Vector3Int, GameObject> spawnedBlocks = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> spawnedFlora = new Dictionary<Vector3Int, GameObject>();
    private HashSet<Vector3> generatedPositions = new HashSet<Vector3>();

    void Update()
    {
        if (player == null || spherePrefab == null) return;

        GenerateTerrainAroundPlayer();
        UnloadDistantTerrain();
        UnloadDistantFlora();
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

                //Uses noise to determine a smooth scale per block.
                //Multiplies position by scale to pack spheres tightly with minimal gaps. 
                float yNoise = Mathf.PerlinNoise(worldX * scale, worldZ * scale) * heightMultiplier;
                int yMax = Mathf.Max(1, Mathf.FloorToInt(yNoise));

                for (int y = 0; y <= yMax; y++)
                {
                    float scaleNoise = Mathf.PerlinNoise((worldX + 100) * 0.2f, (worldZ + 100) * 0.2f);
                    float scale = Mathf.Lerp(minSphereScale, maxSphereScale, scaleNoise);

                    Vector3 pos = new Vector3(worldX * scale, y * scale, worldZ * scale);

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
                    spawnedBlocks[pos] = block;

                    // Flora generation on surface
                    if (y == yMax)
                    {
                        float spawnChance = Random.value;

                        if (blockData.type == SphereType.Grass)
                        {
                            if (spawnChance < 0.05f && treePrefab != null && !spawnedFlora.ContainsKey(pos))
                            {
                                GameObject tree = Instantiate(treePrefab, pos + Vector3.up * 0.5f, Quaternion.identity, transform);
                                spawnedFlora[pos] = tree;
                            }
                            else if (spawnChance < 0.10f && flowerPrefab != null && !spawnedFlora.ContainsKey(pos))
                            {
                                GameObject flower = Instantiate(flowerPrefab, pos + Vector3.up * 0.3f, Quaternion.identity, transform);
                                spawnedFlora[pos] = flower;
                            }
                        }
                        else if (blockData.type == SphereType.Sand)
                        {
                            if (spawnChance < 0.03f && creaturePrefab != null && !spawnedFlora.ContainsKey(pos))
                            {
                                GameObject creature = Instantiate(creaturePrefab, pos + Vector3.up * 0.5f, Quaternion.identity, transform);
                                spawnedFlora[pos] = creature;
                            }
                        }
                    }
                }
            }
        }
    }

    void UnloadDistantTerrain()
    {
        Vector3 playerPos = player.position;
        List<Vector3> toRemove = new List<Vector3>();

        foreach (var pair in spawnedBlocks)
        {
            float distance = Vector3.Distance(pair.Key, playerPos);
            if (distance > unloadRadius)
            {
                Destroy(pair.Value);
                toRemove.Add(pair.Key);
            }
        }

        foreach (var pos in toRemove)
        {
            spawnedBlocks.Remove(pos);
            generatedPositions.Remove(pos);
        }
    }

    void UnloadDistantFlora()
    {
        Vector3 playerPos = player.position;
        List<Vector3> toRemove = new List<Vector3>();

        foreach (var pair in spawnedFlora)
        {
            float distance = Vector3.Distance(pair.Key, playerPos);
            if (distance > unloadRadius)
            {
                Destroy(pair.Value);
                toRemove.Add(pair.Key);
            }
        }

        foreach (var pos in toRemove)
        {
            spawnedFlora.Remove(pos);
        }
    }
}