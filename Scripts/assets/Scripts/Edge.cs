using System;
using UnityEngine;

[System.Serializable]
public class Edge
{
    public int from;
    public int to;
    public float weight;
    public Color color = Color.white;

    public Edge(int from, int to, float weight)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
    }
}
