using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{

    public static bool nextLevel = false;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (nextLevel) {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerEnter(Collider other) {

        if (nextLevel){
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

            int last = 2;

            if (currentSceneIndex == last){
                Application.Quit();

            } else {
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
                nextLevel = false;
            }
        }
    }
}
