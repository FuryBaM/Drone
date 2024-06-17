using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class DuplicateTerrain : MonoBehaviour
{
    [SerializeField] private Material wetMaterial;

    private Terrain terrain;
    private GameObject terrainCopy;
    private Texture2D wetnessMap;
    private int wetnessMapSize = 512;

    private void Start()
    {
        terrain = GetComponent<Terrain>();
        CreateTerrainMeshCopy();
    }

    private void CreateTerrainMeshCopy()
    {
        terrainCopy = new GameObject("TerrainCopy");
        terrainCopy.transform.position = terrain.transform.position;
        terrainCopy.transform.rotation = terrain.transform.rotation;
        terrainCopy.layer = gameObject.layer;

        Mesh terrainMesh = GenerateMeshFromTerrain(terrain);
        MeshFilter meshFilter = terrainCopy.AddComponent<MeshFilter>();
        meshFilter.mesh = terrainMesh;

        MeshRenderer meshRenderer = terrainCopy.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(wetMaterial);

        terrainCopy.AddComponent<MeshCollider>();

        // Hide the original terrain
        terrain.gameObject.SetActive(false);

        // Initialize wetness map
        InitializeWetnessMap();
    }

    private Mesh GenerateMeshFromTerrain(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        float[,] heights = terrainData.GetHeights(0, 0, width, height);

        Vector3 meshScale = terrainData.size;
        Vector2 uvScale = new Vector2(1.0f / (width - 1), 1.0f / (height - 1));
        int vertexCount = width * height;
        int triangleCount = (width - 1) * (height - 1) * 6;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                vertices[y * width + x] = new Vector3(x * meshScale.x / (width - 1), heights[y, x] * meshScale.y, y * meshScale.z / (height - 1));
                uvs[y * width + x] = new Vector2(x * uvScale.x, y * uvScale.y);
            }
        }

        int index = 0;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                triangles[index++] = (y * width) + x;
                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = (y * width) + x + 1;
                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = ((y + 1) * width) + x + 1;
                triangles[index++] = (y * width) + x + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private void InitializeWetnessMap()
    {
        wetnessMap = new Texture2D(wetnessMapSize, wetnessMapSize, TextureFormat.RGBA32, false);
        for (int y = 0; y < wetnessMapSize; y++)
        {
            for (int x = 0; x < wetnessMapSize; x++)
            {
                wetnessMap.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        wetnessMap.Apply();
        terrainCopy.GetComponent<MeshRenderer>().material.SetTexture("_WetnessMap", wetnessMap);
    }
}
