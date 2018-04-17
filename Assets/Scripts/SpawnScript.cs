using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour {

    public GameObject thing;
    

	public void Spawn()
    {
        Instantiate(thing, transform.position, Quaternion.identity);
    }


}
