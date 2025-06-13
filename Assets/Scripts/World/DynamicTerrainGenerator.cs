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

    float baseDiameter = (minSphereScale + maxSphereScale); // consistent spacing
    float horizontalSpacing = baseDiameter * 0.95f; // slight overlap to avoid gaps

    for (int x = -chunkRadius; x <= chunkRadius; x++)
    {
        for (int z = -chunkRadius; z <= chunkRadius; z++)
        {
            int worldX = Mathf.FloorToInt(playerPos.x / horizontalSpacing) + x;
            int worldZ = Mathf.FloorToInt(playerPos.z / horizontalSpacing) + z;

            float noiseHeight = Mathf.PerlinNoise(worldX * scale, worldZ * scale) * heightMultiplier;

            float currentY = 0f;
            float accumulatedHeight = 0f;

            List<(Vector3 pos, float radius, Vector3 key, SphereType type)> sphereStack = new();

            // Fill upward with variable-size spheres, but fixed horizontal spacing
            while (accumulatedHeight < noiseHeight)
            {
                float scaleNoise = Mathf.PerlinNoise((worldX + 1000 + accumulatedHeight) * 0.3f, (worldZ + 1000) * 0.3f);
                float radius = Mathf.Lerp(minSphereScale, maxSphereScale, scaleNoise);
                float diameter = radius * 2f;

                float yPos = currentY + radius;
                Vector3 pos = new Vector3(worldX * horizontalSpacing, yPos, worldZ * horizontalSpacing);
                Vector3 key = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

                if (!generatedPositions.Contains(key))
                    sphereStack.Add((pos, radius, key, GetBiomeType(accumulatedHeight)));

                currentY += diameter;
                accumulatedHeight += diameter;
            }

            if (sphereStack.Count == 0) continue;

            // Spawn only top sphere
            var (topPos, topRadius, topKey, type) = sphereStack[^1];

            GameObject block = terrainPool.Get(topPos, Quaternion.identity, transform);
            block.transform.localScale = Vector3.one * (topRadius * 2f);

            Renderer renderer = block.GetComponent<Renderer>();
            SphereBlock blockData = block.GetComponent<SphereBlock>();
            if (blockData != null)
            {
                blockData.gridPosition = Vector3Int.RoundToInt(topKey);
                blockData.type = type;
            }

            renderer.material = GetMaterialByType(type);

            if (block.GetComponent<Collider>() == null)
                block.AddComponent<SphereCollider>();

            block.isStatic = true;
            block.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
            block.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            generatedPositions.Add(topKey);
            spawnedBlocks[topKey] = block;

            // Flora
            float chance = Random.value;
            if (!spawnedFlora.ContainsKey(topKey))
            {
                if (type == SphereType.Grass)
                {
                    if (chance < 0.05f && treePool)
                        spawnedFlora[topKey] = treePool.Get(topPos + Vector3.up * (topRadius + 0.5f), Quaternion.identity, transform);
                    else if (chance < 0.10f && flowerPool)
                        spawnedFlora[topKey] = flowerPool.Get(topPos + Vector3.up * (topRadius + 0.3f), Quaternion.identity, transform);
                }
                else if (type == SphereType.Sand && chance < 0.03f && creaturePool)
                {
                    spawnedFlora[topKey] = creaturePool.Get(topPos + Vector3.up * (topRadius + 0.5f), Quaternion.identity, transform);
                }
            }
        }
    }
}

SphereType GetBiomeType(float height)
{
    if (height <= 2f) return SphereType.Water;
    if (height < 4f)  return SphereType.Sand;
    if (height < 6f)  return SphereType.Dirt;
    if (height < 8f)  return SphereType.Stone;
    if (height > 8f)  return SphereType.Snow;
    return SphereType.Grass;
}

Material GetMaterialByType(SphereType type)
{
    return type switch
    {
        SphereType.Water => waterMaterial,
        SphereType.Sand => sandMaterial,
        SphereType.Dirt => dirtMaterial,
        SphereType.Stone => stoneMaterial,
        SphereType.Snow => snowMaterial,
        SphereType.Grass => grassMaterial,
        _ => grassMaterial
    };
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
