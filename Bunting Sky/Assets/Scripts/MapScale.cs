using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScale : MonoBehaviour
{
    private GameObject mapModel;

    private void Start()
    {
        mapModel = transform.Find("Map Model").gameObject;
    }

    private void Update()
    {
        mapModel.SetActive(Control.displayMap);
    }
}