public enum TerrainType
{
    TREE,
    BUSH,
    GRASS,
    SAND,
    WATER
}

public class Terrain
{
    public Point point;
    public TerrainType type { get; private set; }

    public Terrain(Point point, TerrainType type)
    {
        this.point = point;
        this.type = type;
    }

    public static TerrainType[] GetEligibleTypes(TerrainType type)
    {
        switch (type)
        {
            case TerrainType.TREE:
                return new TerrainType[] { TerrainType.TREE, TerrainType.BUSH };
            case TerrainType.BUSH:
                return new TerrainType[] { TerrainType.TREE, TerrainType.BUSH, TerrainType.GRASS };
            case TerrainType.GRASS:
                return new TerrainType[] { TerrainType.BUSH, TerrainType.GRASS, TerrainType.SAND };
            case TerrainType.SAND:
                return new TerrainType[] { TerrainType.GRASS, TerrainType.SAND, TerrainType.WATER };
            case TerrainType.WATER:
                return new TerrainType[] { TerrainType.SAND, TerrainType.WATER };
            default:
                return new TerrainType[] { };
        }
    }
}
