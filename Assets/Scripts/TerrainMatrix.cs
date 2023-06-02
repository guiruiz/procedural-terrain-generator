

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Coords
{
    public int x, y;

    public Coords(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class TerrainMatrix
{

    private Terrain[,] matrix;

    public int size { get; private set; }

    public void Initialize(int matrixSize, int startX, int startY, TerrainType startType = TerrainType.WATER)
    {
        size = matrixSize;

        if (!isInRange(startX, startY))
        {
            Debug.Log($"Invalid start coord: {startX}, {startY}");
            return;
        }

        matrix = new Terrain[size, size];


        Terrain startTerrain = new Terrain(startX, startY, startType);
        SetTerrain(startTerrain);

        for (int d = 1; d < matrixSize; d++)
        {
            int rightX = startX + d;
            int leftX = startX - d;

            int upY = startY + d;
            int downY = startY - d;

            // nw -> ne
            for (int x = leftX; x <= rightX; x++)
            {
                CreateTerrain(x, upY);
            }

            // ne V se
            for (int y = upY; y >= downY; y--)
            {
                CreateTerrain(rightX, y);
            }

            // sw <- se
            for (int x = rightX; x >= leftX; x--)
            {
                CreateTerrain(x, downY);
            }

            // sw /\ ne
            for (int y = downY; y <= upY; y++)
            {
                CreateTerrain(leftX, y);
            }
        }
    }


    public Terrain GetTerrain(int x, int y)
    {
        if (!isInRange(x, y))
        {
            //Debug.Log($"GetTerrain on invalid coord  {x}, {y}");
            return null;
        }


        return matrix[x, y];

    }

    Terrain SetTerrain(Terrain terrain)
    {
        matrix[terrain.x, terrain.y] = terrain;
        return terrain;
    }

    void CreateTerrain(int x, int y)
    {
        if (!isInRange(x, y) || GetTerrain(x, y) != null)
        {
            return;
        }

        int[][] adjacentCoords = GetAdjacentCoords(x, y);
        Terrain[] validAdjacents = GetValidTerrains(adjacentCoords);

        TerrainType terrainType = PickTerrainType(validAdjacents);
        Terrain terrain = new Terrain(x, y, terrainType);
        SetTerrain(terrain);
    }

    Terrain[] GetValidTerrains(int[][] coords)
    {
        List<Terrain> result = new List<Terrain>();
        foreach (int[] c in coords)
        {
            if (!isInRange(c[0], c[1])) { continue; }

            Terrain t = GetTerrain(c[0], c[1]);
            if (t != null) { result.Add(t); }
        }

        return result.ToArray();
    }

    int[][] GetAdjacentCoords(int startX, int startY, int d = 1)
    {
        int[] n = { startX, startY + d };
        int[] s = { startX, startY - d };
        int[] e = { startX + d, startY };
        int[] w = { startX - d, startY };
        int[] ne = { startX + d, startY + d };
        int[] nw = { startX - d, startY + d };
        int[] se = { startX + d, startY - d };
        int[] sw = { startX - d, startY - d };

        int[][] ajds = { n, ne, e, se, s, sw, w, nw };
        return ajds;
    }

    TerrainType PickTerrainType(Terrain[] adjacents)
    {
        if (adjacents.Length == 0)
        {
            TerrainType initialType = GetRandomTerrainType();
            Debug.Log($"Initial type: {initialType}");
            return initialType;
        }

        // get possible terrains types for each adj
        TerrainType[][] allPossibleTypes = Array.ConvertAll(adjacents, adj => Terrain.GetPossibleTypes(adj.type));

        // find possible terrains intersection
        IEnumerable<TerrainType> intersection = allPossibleTypes
        .Skip(1) // Skip the first array
        .Aggregate(
            new HashSet<TerrainType>(allPossibleTypes.First()),
            (result, arr) => { result.IntersectWith(arr); return result; }
        );

        // @todo check why possibleTypes is empty sometimes 
        TerrainType[] possibleTypes = intersection.ToArray();

        TerrainType pickedTerrain;
        try
        {
            // pick random possible terrain
            int randomIndex = UnityEngine.Random.Range(0, possibleTypes.Length);
            pickedTerrain = possibleTypes[randomIndex];
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log($"-----------------------------");
            Debug.Log($"All possible");
            foreach (TerrainType[] z in allPossibleTypes)
            {
                Debug.Log(string.Join(" ", z));
            }
            Debug.Log($"-----------------------------");
            Debug.Log($"Intersection");
            Debug.Log(string.Join(" ", intersection));
            Debug.Log($"-----------------------------");
            Debug.Log($"Possible");
            Debug.Log(string.Join(" ", possibleTypes));
            Debug.Log($"-----------------------------");

            throw e;
        }



        //TerrainType pickedTerrain = possibleTypes.Length > 0 ? possibleTypes[randomIndex] : GetRandomTerrainType(true);

        return pickedTerrain;
    }

    TerrainType GetRandomTerrainType(bool log = false)
    {
        TerrainType randomTerrainType = (TerrainType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(TerrainType)).Length);

        if (log)
        {
            Debug.Log($"Random terrain type: {log}");
        }

        return randomTerrainType;
    }


    bool isInRange(int x, int y)
    {
        return x >= 0 && y >= 0 && x < size && y < size;
    }

}
