

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainMatrix
{
    private Terrain[,] matrix;

    public int matrixSize { get; private set; }

    public void Initialize(int matrixSize, int startX, int startY, TerrainType startType = TerrainType.WATER)
    {
        this.matrixSize = matrixSize;
        Point startPoint = new Point(startX, startY);

        if (!IsInRange(startPoint))
        {
            Debug.Log($"Invalid start coord: {startPoint}");
            return;
        }

        matrix = new Terrain[this.matrixSize, this.matrixSize];
        Terrain startTerrain = new Terrain(startPoint, startType);
        SetTerrain(startTerrain);

        GenerateTerrains(startPoint);
    }

    public Terrain GetTerrain(Point point)
    {
        if (!IsInRange(point)) { return null; }

        return matrix[point.x, point.y];
    }

    Terrain SetTerrain(Terrain terrain)
    {
        matrix[terrain.point.x, terrain.point.y] = terrain;
        return terrain;
    }

    void GenerateTerrains(Point startPoint)
    {
        for (int d = 1; d < matrixSize; d++)
        {
            int rightX = startPoint.x + d;
            int leftX = startPoint.x - d;

            int upY = startPoint.y + d;
            int downY = startPoint.y - d;

            // nw -> ne
            for (int x = leftX; x <= rightX; x++)
            {
                CreateTerrain(new Point(x, upY));
            }

            // ne V se
            for (int y = upY; y >= downY; y--)
            {
                CreateTerrain(new Point(rightX, y));
            }

            // sw <- se
            for (int x = rightX; x >= leftX; x--)
            {
                CreateTerrain(new Point(x, downY));
            }

            // sw /\ ne
            for (int y = downY; y <= upY; y++)
            {
                CreateTerrain(new Point(leftX, y));
            }
        }
    }


    void CreateTerrain(Point point)
    {
        if (!IsInRange(point) || GetTerrain(point) != null)
        {
            return;
        }

        Point[] adjacentPoints = GetAdjacentPoints(point);


        Terrain[] adjacentTerrains = adjacentPoints.Select(p => GetTerrain(p)).Where(p => p != null).ToArray();

        TerrainType terrainType = this.PickTerrainType(adjacentTerrains);
        Terrain terrain = new Terrain(point, terrainType);
        SetTerrain(terrain);
    }

    Point[] GetAdjacentPoints(Point point)
    {
        int d = 1;
        Point n = new Point(point.x, point.y + d);
        Point s = new Point(point.x, point.y - d);
        Point e = new Point(point.x + d, point.y);
        Point w = new Point(point.x - d, point.y);
        Point ne = new Point(point.x + d, point.y + d);
        Point nw = new Point(point.x - d, point.y + d);
        Point se = new Point(point.x + d, point.y - d);
        Point sw = new Point(point.x - d, point.y - d);

        return new Point[] { n, ne, e, se, s, sw, w, nw }; ;
    }

    TerrainType PickTerrainType(Terrain[] adjacents)
    {
        if (adjacents.Length == 0)
        {
            Debug.Log($"WARN adj terrain not found;");
            return GetRandomTerrainType();
        }

        // get all eligible terrains types for each adj
        TerrainType[][] allEligibleTypes = Array.ConvertAll(adjacents, adj => Terrain.GetEligibleTypes(adj.type));

        // find eligible terrain types intersection
        TerrainType[] eligibleTypes = allEligibleTypes
        .Skip(1) // Skip the first array
        .Aggregate(
            new HashSet<TerrainType>(allEligibleTypes.First()),
            (result, arr) => { result.IntersectWith(arr); return result; }
        ).ToArray();

        // pick random possible terrain
        int randomIndex = UnityEngine.Random.Range(0, eligibleTypes.Length);
        TerrainType terrain = eligibleTypes[randomIndex];

        // @todo check if empty?:  
        //TerrainType terrain = possibleTypes.Length > 0 ? eligibleTypes[randomIndex] : GetRandomTerrainType(true);

        return terrain;
    }

    TerrainType GetRandomTerrainType()
    {
        TerrainType randomTerrainType = (TerrainType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(TerrainType)).Length);
        return randomTerrainType;
    }

    bool IsInRange(Point p)
    {
        return p.x >= 0 && p.y >= 0 && p.x < matrixSize && p.y < matrixSize;
    }
}
