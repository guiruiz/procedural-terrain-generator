using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject terrainPrefab;
    public Material treeMaterial;
    public Material bushMaterial;
    public Material grassMaterial;
    public Material sandMaterial;
    public Material waterMaterial;
    public Material unknownMaterial;
    public int matrixSize = 3; // @todo odd only?
    public int startX = 1;
    public int startY = 1;
    public TerrainType startType = TerrainType.WATER;
    public PickTypeStrategy pickTypeStrategy = PickTypeStrategy.Random;
    public bool generateTerrain = false;


    private float terrainPrefabSize = 1f;
    void Start()
    {
        GenerateTerrains();
    }


    void Update()
    {
        if (generateTerrain)
        {
            DestroyTerrains();
            GenerateTerrains();

            generateTerrain = false;
        }
    }

    void GenerateTerrains()
    {
        TerrainMatrix matrix = new TerrainMatrix();
        matrix.Initialize(matrixSize, startX, startY, startType, pickTypeStrategy);

        for (int x = 0; x < matrixSize; x++)
        {
            string rowLog = "";
            for (int y = 0; y < matrixSize; y++)
            {
                Point point = new Point(x, y);
                Terrain terrain = matrix.GetTerrain(point);
                rowLog += (terrain != null) ? $" {terrain?.type} -" : " NULL -";
                SpawnTerrain(terrain);
            }
            //Debug.Log($"Row {x} = {rowLog}");
        }
    }

    void SpawnTerrain(Terrain terrain)
    {
        if (terrain == null) { return; }
        Vector3 position = new Vector3(
            transform.position.x + terrain.point.x * terrainPrefabSize,
            0f,
            transform.position.z + terrain.point.y * terrainPrefabSize);

        GameObject terrainObject = Instantiate(terrainPrefab, position, Quaternion.identity);
        terrainObject.transform.SetParent(transform);

        MeshRenderer meshRenderer = terrainObject.GetComponent<MeshRenderer>();
        meshRenderer.material = GetTerrainMaterial(terrain.type);
    }

    Material GetTerrainMaterial(TerrainType type)
    {
        switch (type)
        {
            case TerrainType.TREE:
                return treeMaterial;
            case TerrainType.BUSH:
                return bushMaterial;
            case TerrainType.GRASS:
                return grassMaterial;
            case TerrainType.SAND:
                return sandMaterial;
            case TerrainType.WATER:
                return waterMaterial;
            default:
                return unknownMaterial;
        }
    }

    private void DestroyTerrains()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
