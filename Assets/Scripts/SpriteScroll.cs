using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteScroll : MonoBehaviour
{
    public float scrollspeed = 1f;
    public float multiplier = 6f;

    private float rightedge;
    private float leftedge;
    private Vector3 distance;

    void Start()
    {
        var renderer = GetComponent<SpriteRenderer>();
        rightedge = transform.position.x + renderer.bounds.extents.x / (multiplier / 2f);
        leftedge = transform.position.x - renderer.bounds.extents.x / (multiplier / 2f);
        distance = new Vector3(rightedge - leftedge, 0f, 0f);
    }

    void Update()
    {
        transform.localPosition += scrollspeed * Vector3.right * Time.deltaTime;

        if (scrollspeed > 0 && transform.position.x > rightedge)
        {
            transform.position -= distance;
        }
        else if (scrollspeed < 0 && transform.position.x < leftedge)
        {
            transform.position += distance;
        }
    }
}
