using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EdgeWeightDisplay : MonoBehaviour
{
    public Text weightText;
    public float offset = 0.3f; // Смещение от центра ребра

    public void Initialize(float weight, Vector3 position)
    {
        if (weightText != null)
        {
            weightText.text = weight.ToString("F1");
            weightText.transform.position = position;
        }
    }

    public void SetColor(Color color)
    {
        if (weightText != null)
        {
            weightText.color = color;
        }
    }
}
