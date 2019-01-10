using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate : MonoBehaviour {

    public Transform spawnPos;
    public GameObject spawnee;

    // Update is called once per frame

    private void Start()
    {
      
    }
    void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(spawnee, spawnPos.position, spawnPos.rotation);
        }

    }

}
