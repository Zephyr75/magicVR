using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    private int num_ingredients = 0;
    private List<string> ingredients = new List<string>();

    [SerializeField]
    private GameObject potion, smoke;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DeactivatePotion());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision){
        if (collision.gameObject.transform.GetComponent<ObjectAnchor>() != null && !ingredients.Contains(collision.gameObject.name)){
            collision.gameObject.transform.parent = gameObject.transform;
            collision.gameObject.GetComponent<ObjectAnchor>().graspingRadius = 0f;
            num_ingredients++;
            ingredients.Add(collision.gameObject.name);
        }
        if (collision.gameObject.name == "campfire" && num_ingredients >= 3){
            StartCoroutine(Smoke());
            num_ingredients = 0;
        }
    }

    IEnumerator Smoke(){
        smoke.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        // Deactivate all ingredients
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            child.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        // Hide the cauldron
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<BoxCollider>().enabled = false;

        potion.SetActive(true);
        LoadLevel.nextLevel = true;

        yield return new WaitForSeconds(0.5f);
        Destroy(smoke);

        foreach (Transform child in potion.transform)
        {
            child.gameObject.GetComponent<BoxCollider>().enabled = true;
            child.gameObject.GetComponent<Rigidbody>().useGravity = true;
        }

        gameObject.SetActive(false);


    }

    IEnumerator DeactivatePotion(){
        yield return new WaitForSeconds(0.5f);
        potion.SetActive(false);
    }
}
