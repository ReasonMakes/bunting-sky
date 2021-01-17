using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxStarsControl : MonoBehaviour
{
    public GameObject starObj;
    private GameObject starInstance;
    private float starCount = 400f;

    // Start is called before the first frame update
    void Start()
    {
        //Instantiate(starObj, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        //starInstance = Instantiate(starObj, Vector3.zero, Quaternion.identity);

        for (int i = 0; i < starCount; i++)
        {
            starInstance = Instantiate(starObj, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            starInstance.transform.SetParent(transform);
        }
        
        //starInstance.PlaceInSkysphere();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
