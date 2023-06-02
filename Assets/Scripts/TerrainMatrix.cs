

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainMatrix
{

    private Terrain[,] matrix;

    public int size { get; private set; }

    public void Initialize(int matrixSize, int startX, int startY)
    {
        size = matrixSize;

        if (!isValidCoord(startX, startY))
        {
            Debug.Log($"Invalid start coord: {startX}, {startY}");
            return;
        }

        matrix = new Terrain[size, size];

        CreateTerrain(startX, startY);
    }

    public Terrain GetTerrain(int x, int y)
    {
        if (!isValidCoord(x, y))
        {
            //Debug.Log($"GetTerrain on invalid coord  {x}, {y}");
            return null;
        }


        try
        {
            return matrix[x, y];
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log($"IndexOutOfRangeException at {x}, {y}");
            return null;
        }
    }

    Terrain SetTerrain(Terrain terrain)
    {
        matrix[terrain.x, terrain.y] = terrain;
        return terrain;
    }

    void CreateTerrain(int x, int y)
    {
        if (!isValidCoord(x, y)) { return; }

        int[][] adjacentCoords = GetAdjacentCoords(x, y);
        Terrain[] validAdjacents = GetValidTerrains(adjacentCoords);

        TerrainType terrainType = PickTerrainType(validAdjacents);
        Terrain terrain = new Terrain(x, y, terrainType);
        SetTerrain(terrain);

        foreach (int[] c in adjacentCoords)
        {
            if (!isValidCoord(x, y)) { continue; }

            Terrain t = GetTerrain(c[0], c[1]);
            if (t == null)
            {
                CreateTerrain(c[0], c[1]);
            }
        }

    }

    Terrain[] GetValidTerrains(int[][] coords)
    {
        List<Terrain> result = new List<Terrain>();
        foreach (int[] c in coords)
        {
            if (!isValidCoord(c[0], c[1])) { continue; }

            Terrain t = GetTerrain(c[0], c[1]);
            if (t != null) { result.Add(t); }
        }

        return result.ToArray();
    }

    int[][] GetAdjacentCoords(int startX, int startY)
    {
        int[] n = { startX, startY + 1 };
        int[] s = { startX, startY - 1 };
        int[] e = { startX + 1, startY };
        int[] w = { startX - 1, startY };
        int[] ne = { startX + 1, startY + 1 };
        int[] nw = { startX - 1, startY + 1 };
        int[] se = { startX + 1, startY - 1 };
        int[] sw = { startX - 1, startY - 1 };

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


    bool isValidCoord(int x, int y)
    {
        return x >= 0 && y >= 0 && x < size && y < size;
    }

}
