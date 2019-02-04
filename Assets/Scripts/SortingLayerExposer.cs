using UnityEngine;
using System.Collections;

/// <summary>
/// We need this for certain unity objects which don't allow you to edit their sorting layer for some stupid reason.
/// </summary>
public class SortingLayerExposer : MonoBehaviour
{
    public string SortingLayerName = "Default";
    public int SortingOrder = 0;

    void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = SortingLayerName;
        gameObject.GetComponent<MeshRenderer>().sortingOrder = SortingOrder;
    }
}
