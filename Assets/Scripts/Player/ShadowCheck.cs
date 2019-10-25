using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCheck : MonoBehaviour
{
    Vector2 lightPos;
    Transform[] lightingTransforms;
    GameObject[] lightingObjects;
    LayerMask mask;

    void Start()
    {
        lightingObjects = GameObject.FindGameObjectsWithTag("Lighting");
        lightingTransforms = new Transform[lightingObjects.Length];
        mask = (1 << 9) + (1 << 15); //9 - platform layer, 15 - shadow platform layer

        for (int i = 0; i < lightingObjects.Length; i++)
        {
            lightingTransforms[i] = lightingObjects[i].GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        lightPos = (lightingTransforms[0].position - transform.position);
        
        RaycastHit2D ray = Physics2D.Raycast(transform.position, lightPos, Mathf.Infinity, mask);
        Debug.DrawRay(transform.position, lightPos);

        if (ray.transform == null)
            Debug.Log("Under sun");
        else
            Debug.Log("In the shadows" + ray.transform.name);
    }
}
