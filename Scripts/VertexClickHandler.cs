using UnityEngine;

public class VertexClickHandler : MonoBehaviour
{
    private GraphController graphController;
    private int vertexId;

    public void Initialize(GraphController controller, int id)
    {
        graphController = controller;
        vertexId = id;

        // Добавляем коллайдер если его нет
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }
    }

    void OnMouseDown()
    {
        if (graphController != null)
        {
            graphController.OnVertexClicked(vertexId);
        }
        else
        {
            Debug.LogError("GraphController not initialized!");
        }
    }
}