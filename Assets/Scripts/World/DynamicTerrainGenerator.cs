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

    for (int x = -chunkRadius; x <= chunkRadius; x++)
    {
        for (int z = -chunkRadius; z <= chunkRadius; z++)
        {
            float noiseX = (playerPos.x + x) * scale;
            float noiseZ = (playerPos.z + z) * scale;

            float surfaceHeight = Mathf.PerlinNoise(noiseX, noiseZ) * heightMultiplier;
            int ySteps = Mathf.CeilToInt(surfaceHeight);

            float baseX = Mathf.Floor(playerPos.x / 1f) + x;
            float baseZ = Mathf.Floor(playerPos.z / 1f) + z;

            float cumulativeHeight = 0f;

            for (int y = 0; y <= ySteps; y++)
            {
                float radiusNoise = Mathf.PerlinNoise((baseX + 100) * 0.15f, (baseZ + 100) * 0.15f);
                float radius = Mathf.Lerp(minSphereScale, maxSphereScale, radiusNoise);
                float diameter = radius * 2f;

                float averageDiameter = (minSphereScale + maxSphereScale); // 0.8 + 1.2 = 2
                Vector3 pos = new Vector3(baseX * averageDiameter * 0.95f, cumulativeHeight, baseZ * averageDiameter * 0.95f);

                Vector3 key = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

                if (generatedPositions.Contains(key)) continue;

                GameObject block = terrainPool.Get(pos, Quaternion.identity, transform);
                block.transform.localScale = Vector3.one * diameter;

                Renderer renderer = block.GetComponent<Renderer>();
                SphereBlock blockData = block.GetComponent<SphereBlock>();
                if (blockData != null)
                {
                    blockData.gridPosition = Vector3Int.RoundToInt(key);
                    blockData.type = GetBlockTypeFromHeight(y);
                }

                renderer.material = GetMaterialForType(blockData?.type ?? SphereType.Dirt);

                if (block.GetComponent<Collider>() == null)
                    block.AddComponent<SphereCollider>();

                block.isStatic = true;
                block.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
                block.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                generatedPositions.Add(key);
                spawnedBlocks[key] = block;

                // Stack next sphere above the current one
                cumulativeHeight += diameter * 0.95f; // small overlap to prevent floating
            }
        }
    }
}

SphereType GetBlockTypeFromHeight(int y)
{
    if (y <= 2) return SphereType.Water;
    if (y < 4) return SphereType.Sand;
    if (y < 6) return SphereType.Dirt;
    if (y < 8) return SphereType.Stone;
    if (y > 8) return SphereType.Snow;
    return SphereType.Grass;
}

Material GetMaterialForType(SphereType type)
{
    return type switch
    {
        SphereType.Water => waterMaterial,
        SphereType.Sand => sandMaterial,
        SphereType.Dirt => dirtMaterial,
        SphereType.Stone => stoneMaterial,
        SphereType.Snow => snowMaterial,
        SphereType.Grass => grassMaterial,
        _ => dirtMaterial,
    };
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
