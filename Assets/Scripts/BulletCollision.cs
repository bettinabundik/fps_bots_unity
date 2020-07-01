using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    public GameObject explosiontemplate;

    void Start()
    { }

    void Update()
    { }

    private void OnTriggerEnter(Collider other)
    {
        // When a laserbullet hits a wall
        if (other.CompareTag("laserbullet"))
        {
            // Play explosion effect
            GameObject explosion = BulletCollision.Instantiate(explosiontemplate,
                new Vector3(other.transform.position.x, other.transform.position.y, other.transform.position.z), 
                "explosion");
            explosion.transform.Rotate(new Vector3(-90f, 0f, 0f));

            // Destroy bullet gameobject
            Destroy(other.gameObject);
        }
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 position, string tag)
    {
        var gameobject = GameObject.Instantiate(prefab, position, Quaternion.identity) as GameObject;
        gameobject.tag = tag;
        return gameobject;
    }
}
