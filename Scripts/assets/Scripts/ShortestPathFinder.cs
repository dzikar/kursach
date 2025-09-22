using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class ShortestPathFinder
    {
        public static (float[,] distances, int[,] predecessors) FloydWarshall(Graph graph)
        {
            graph.BuildAdjacencyMatrix();
            float[,] dist = (float[,])graph.GetAdjacencyMatrix().Clone();
            int vertexCount = graph.vertices.Count;

            int[,] next = new int[vertexCount, vertexCount];

            // Инициализация матрицы предшественников
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    if (dist[i, j] != float.PositiveInfinity && i != j)
                        next[i, j] = j;
                    else
                        next[i, j] = -1;
                }
            }

            // Алгоритм Флойда-Уоршелла
            for (int k = 0; k < vertexCount; k++)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    for (int j = 0; j < vertexCount; j++)
                    {
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            next[i, j] = next[i, k];
                        }
                    }
                }
            }

            return (dist, next);
        }

        public static List<int> ReconstructPath(int[,] next, int start, int end)
        {
            if (next[start, end] == -1)
                return new List<int>();

            List<int> path = new List<int> { start };

            while (start != end)
            {
                start = next[start, end];
                path.Add(start);
            }

            return path;
        }
    }

