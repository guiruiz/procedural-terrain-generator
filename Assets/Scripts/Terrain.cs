
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
    public int x { get; private set; }
    public int y { get; private set; }
    public TerrainType type { get; private set; }

    public Terrain(int xPos, int yPos, TerrainType type)
    {
        this.x = xPos;
        this.y = yPos;
        this.type = type;
    }

    public static TerrainType[] GetPossibleTypes(TerrainType type)
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
