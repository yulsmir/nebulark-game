using System.Collections.Generic;
using UnityEngine;
using World;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Noise Settings")]
    public float scale = 0.05f;
    public float heightMultiplier = 10f;
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
    public float minSphereScale = 1f;
    public float maxSphereScale = 1.4f;
    public float fillerMinScale = 0.1f;
    public float fillerMaxScale = 0.5f;
    public int fillersPerGap = 3;

    [Header("Player Reference")]
    public Transform player;

    [Header("Unloading Settings")]
    public int unloadRadius = 14;

    [Header("Procedural Content")]
    public ObjectPooler terrainPool;
    public ObjectPooler treePool;
    public ObjectPooler flowerPool;
    public ObjectPooler creaturePool;
    public ObjectPooler fillerPool;

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
                int worldX = Mathf.FloorToInt(playerPos.x) + x;
                int worldZ = Mathf.FloorToInt(playerPos.z) + z;

                float noiseY = Mathf.PerlinNoise(worldX * scale, worldZ * scale) * heightMultiplier;
                int y = Mathf.FloorToInt(noiseY);

                float scaleNoise = Mathf.PerlinNoise((worldX + 1000) * 0.2f, (worldZ + 1000) * 0.2f);
                float radius = Mathf.Lerp(minSphereScale, maxSphereScale, scaleNoise);
                float diameter = radius * 2f;

                float overlapFactor = 0.75f;
                float spacing = diameter * overlapFactor;

                Vector3 pos = new Vector3(worldX * spacing, y * spacing, worldZ * spacing);
                Vector3 key = RoundVector3(pos);

                if (generatedPositions.Contains(key)) continue;

                GameObject block = terrainPool.Get(pos, Quaternion.identity, transform);
                block.transform.localScale = Vector3.one * diameter;

                Renderer renderer = block.GetComponent<Renderer>();
                SphereBlock blockData = block.GetComponent<SphereBlock>();
                if (blockData != null) blockData.gridPosition = Vector3Int.RoundToInt(key);

                float normalizedHeight = y / heightMultiplier;
                SphereType type = SphereType.Grass;

                if (normalizedHeight < 0.2f)
                {
                    type = SphereType.Water;
                    renderer.material = waterMaterial;
                }
                else if (normalizedHeight < 0.35f)
                {
                    type = SphereType.Sand;
                    renderer.material = sandMaterial;
                }
                else if (normalizedHeight < 0.5f)
                {
                    type = SphereType.Dirt;
                    renderer.material = dirtMaterial;
                }
                else if (normalizedHeight < 0.7f)
                {
                    type = SphereType.Grass;
                    renderer.material = grassMaterial;
                }
                else if (normalizedHeight < 0.9f)
                {
                    type = SphereType.Stone;
                    renderer.material = stoneMaterial;
                }
                else
                {
                    type = SphereType.Snow;
                    renderer.material = snowMaterial;
                }

                if (blockData != null) blockData.type = type;

                if (block.GetComponent<Collider>() == null)
                    block.AddComponent<SphereCollider>();

                block.isStatic = true;
                block.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
                block.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                generatedPositions.Add(key);
                spawnedBlocks[key] = block;

                TrySpawnFlora(type, key, pos, radius);
                FillGapsAround(pos, spacing * 0.6f, type);
            }
        }
    }

    void TrySpawnFlora(SphereType type, Vector3 key, Vector3 pos, float radius)
    {
        float chance = Random.value;
        if (spawnedFlora.ContainsKey(key)) return;

        if (type == SphereType.Grass)
        {
            if (chance < 0.05f && treePool)
                spawnedFlora[key] = treePool.Get(pos + Vector3.up * radius, Quaternion.identity, transform);
            else if (chance < 0.10f && flowerPool)
                spawnedFlora[key] = flowerPool.Get(pos + Vector3.up * radius * 0.8f, Quaternion.identity, transform);
        }
        else if (type == SphereType.Sand && chance < 0.03f && creaturePool)
        {
            spawnedFlora[key] = creaturePool.Get(pos + Vector3.up * radius, Quaternion.identity, transform);
        }
    }

    void FillGapsAround(Vector3 center, float radius, SphereType biome)
    {
        for (int i = 0; i < fillersPerGap; i++)
        {
            Vector3 offset = Random.insideUnitSphere * radius;
            Vector3 fillerPos = center + offset;
            Vector3 key = RoundVector3(fillerPos);
            if (generatedPositions.Contains(key)) continue;

            GameObject filler = fillerPool.Get(fillerPos, Quaternion.identity, transform);
            float scale = Random.Range(fillerMinScale, fillerMaxScale);
            filler.transform.localScale = Vector3.one * scale;

            Renderer renderer = filler.GetComponent<Renderer>();
            SphereBlock blockData = filler.GetComponent<SphereBlock>();
            if (blockData != null)
            {
                blockData.gridPosition = Vector3Int.RoundToInt(key);
                blockData.type = biome;
            }

            renderer.material = GetMaterialForBiome(biome);

            if (filler.GetComponent<Collider>() == null)
                filler.AddComponent<SphereCollider>();

            generatedPositions.Add(key);
            spawnedBlocks[key] = filler;
        }
    }

    Material GetMaterialForBiome(SphereType type)
    {
        return type switch
        {
            SphereType.Grass => grassMaterial,
            SphereType.Dirt => dirtMaterial,
            SphereType.Stone => stoneMaterial,
            SphereType.Water => waterMaterial,
            SphereType.Sand => sandMaterial,
            SphereType.Snow => snowMaterial,
            _ => grassMaterial,
        };
    }

    Vector3 RoundVector3(Vector3 vec)
    {
        return new Vector3(Mathf.Round(vec.x), Mathf.Round(vec.y), Mathf.Round(vec.z));
    }

    void UnloadDistantTerrain()
    {
        Vector3 playerPos = player.position;
        List<Vector3> toRemove = new();

        foreach (var pair in spawnedBlocks)
        {
            if (Vector3.Distance(pair.Key, playerPos) > unloadRadius * 2f)
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
            if (Vector3.Distance(pair.Key, playerPos) > unloadRadius * 2f)
            {
                if (pair.Value.name.Contains("Tree")) treePool?.Return(pair.Value);
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
