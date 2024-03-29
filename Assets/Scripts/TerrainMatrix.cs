

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum PickTypeStrategy
{
    Random,
    RandomWeighted,
}
public class TerrainMatrix
{
    private Terrain[,] matrix;
    public int startXxx = 1;

    public int matrixSize { get; private set; }
    public PickTypeStrategy pickTypeStrategy { get; private set; }


    public void Initialize(int matrixSize, int startX, int startY, TerrainType startType, PickTypeStrategy pickTypeStrategy)
    {
        this.matrixSize = matrixSize;
        this.pickTypeStrategy = pickTypeStrategy;

        Point startPoint = new Point(startX, startY);

        if (!IsInRange(startPoint))
        {
            Debug.Log($"Invalid start coord: {startPoint}");
            return;
        }

        // initialize matrix
        matrix = new Terrain[this.matrixSize, this.matrixSize];

        // Set initial terrain
        SetTerrain(new Terrain(startPoint, startType));

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
            for (int y = upY - 1; y >= downY; y--)
            {
                CreateTerrain(new Point(rightX, y));
            }

            // sw <- se
            for (int x = rightX - 1; x >= leftX; x--)
            {
                CreateTerrain(new Point(x, downY));
            }

            // sw /\ ne
            for (int y = downY + 1; y <= upY; y++)
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
            Debug.LogWarning($"Adjacent terrains not found, picking a random type.");
            return GetRandomTerrainType();
        }

        TerrainType[] adjacentTypes = Array.ConvertAll(adjacents, adj => adj.type);

        // get eligible terrain types for all adjacents
        TerrainType[][] allEligibleTypes = Array.ConvertAll(adjacents, adj => Terrain.GetEligibleTypes(adj.type));

        // find eligible terrain types from the intersection
        TerrainType[] eligibleTypes = allEligibleTypes
        .Skip(1) // Skip the first array
        .Aggregate(
            new HashSet<TerrainType>(allEligibleTypes.First()),
            (result, arr) => { result.IntersectWith(arr); return result; }
        ).ToArray();


        if (this.pickTypeStrategy == PickTypeStrategy.Random)
        {
            // pick a random eligible terrain
            int randomIndex = UnityEngine.Random.Range(0, eligibleTypes.Length);
            TerrainType randomType = eligibleTypes[randomIndex];
            return randomType;
        }

        if (this.pickTypeStrategy == PickTypeStrategy.RandomWeighted)
        {
            // @todo play more with weightMod and random selection
            float weightMod = .275f;
            var adjacentTypesWeighted = adjacentTypes.GroupBy(type => type).Select(group => new { type = group.Key, weight = 1 - (group.Count() * weightMod) });
            var eligiblesWeighted = Array.ConvertAll(eligibleTypes, t =>
            {
                // try to find eligible weighted type
                var weightedType = adjacentTypesWeighted.FirstOrDefault(a => a.type == t);
                // set default weight if unable to find
                if (weightedType == null) { weightedType = new { type = t, weight = 1f }; }
                return weightedType;
            });


            // Get a random object using weight
            var random = new System.Random();
            var randomWeighted = eligiblesWeighted
                .OrderBy(item => random.NextDouble() * item.weight)
                .First();

            //@ todo remove debug log
            // Debug.Log($"---------------------------------------eligibleTypes");
            // foreach (var t in eligibleTypes)
            // {
            //     Debug.Log($"---- Type: {t}");
            // }
            // Debug.Log($"--------adjacentsWeighted");
            // foreach (var group in adjacentsWeighted)
            // {
            //     Debug.Log($"---- Type: {group.type}, Weight: {group.weight}");
            // }
            // Debug.Log($"--------eligiblesWeighted");
            // foreach (var group in eligiblesWeighted)
            // {
            //     Debug.Log($"---- Type: {group.type}, Weight: {group.weight}");
            // }
            // Debug.Log($"--------");
            // Debug.Log($"Selected; {randomWeighted.type}");
            // Debug.Log($"---------------------------------------");

            return randomWeighted.type;
        }

        throw new Exception("PickTypeStrategy not set");
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
