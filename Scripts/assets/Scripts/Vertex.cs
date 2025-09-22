using System;
using UnityEngine;

[System.Serializable]
public class Vertex
{
    public int id;
    public string name;
    public Vector2 position;
    public Color color = Color.white;
    [NonSerialized] public GameObject labelObject;

    public Vertex(int id, string name, Vector2 position)
    {
        this.id = id;
        this.name = name;
        this.position = position;
    }
}