using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomProp : MonoBehaviour
{
	public Transform PropLocation;
	public List<GameObject> PropPrefabs;
	public float PropChance;

    // Start is called before the first frame update
    void Start()
    {
		if (Random.value < PropChance)
		{
			Instantiate(PropPrefabs.RandomElement(), PropLocation.position, PropLocation.rotation, transform);
		}
    }

    // Update is called once per frame
    void Update()
    {

    }
}
