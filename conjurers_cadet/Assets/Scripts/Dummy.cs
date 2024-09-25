using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
    void Update()
    {
        rb.centerOfMass = new Vector3(0, 0, -1);
    }

    void OnCollisionEnter(Collision collision)
    {
        // ceck that no terrain tag
        if(collision.gameObject.tag != "Terrain" && gameObject.GetComponent<Rigidbody>().velocity.magnitude > 0.5f)
        {
            audioSource.Play();
            LoadLevel.nextLevel = true;
        }
    }
}
