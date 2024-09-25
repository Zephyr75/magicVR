using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private GameObject explosion;
    [SerializeField] 
    public bool shadow = false;
    private AudioSource audioSource;
    private bool exploded = false;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
		Debug.LogWarningFormat( "collided with {0}", collision.gameObject.name);

        if (collision.gameObject.layer != 6 && gameObject.GetComponent<Rigidbody>().velocity.magnitude > .5f && !exploded)
        {
            StartCoroutine(Explode());
        }
    }

    void OnTriggerEnter(Collider other)
    {
    }


    IEnumerator Explode()
    {
        exploded = true;
        GameObject explosionInstance = Instantiate(explosion, transform.position, transform.rotation);

        // play clip at location
        AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
        OVRInput.SetControllerVibration(1, 2, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(1, 2, OVRInput.Controller.LTouch);

        if (shadow){
            foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }
        }

        yield return new WaitForSeconds(0.2f);
        OVRInput.SetControllerVibration(1, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(1, 0, OVRInput.Controller.LTouch);

        yield return new WaitForSeconds(1.5f);




        Destroy(explosionInstance);
        gameObject.SetActive(false);
    }
    
}
