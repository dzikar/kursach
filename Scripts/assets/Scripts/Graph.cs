using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Graph
{
    public List<Vertex> vertices = new List<Vertex>();
    public List<Edge> edges = new List<Edge>();

    private float[,] adjacencyMatrix;

    public void BuildAdjacencyMatrix()
    {
        int count = vertices.Count;
        adjacencyMatrix = new float[count, count];

        // Инициализация матрицы бесконечностями
        for (int i = 0; i < count; i++)
            for (int j = 0; j < count; j++)
                adjacencyMatrix[i, j] = float.PositiveInfinity;

        // Заполнение матрицы весами рёбер
        foreach (var edge in edges)
        {
            adjacencyMatrix[edge.from, edge.to] = edge.weight;
        }

        // Диагональные элементы = 0
        for (int i = 0; i < count; i++)
            adjacencyMatrix[i, i] = 0;
    }

    public float[,] GetAdjacencyMatrix()
    {
        return adjacencyMatrix;
    }

    public Vertex GetVertex(int id)
    {
        return vertices.Find(v => v.id == id);
    }
}