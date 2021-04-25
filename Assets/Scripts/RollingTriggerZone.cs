using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingTriggerZone : MonoBehaviour
{
	private RollingDungeonGenerator linkedGenerator;
	private int myDepth;

	public void LinkGenerator(RollingDungeonGenerator generator, int depth)
	{
		linkedGenerator = generator;
		myDepth = depth;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (linkedGenerator && other.GetComponent<CharacterController>())
		{
			linkedGenerator.OnDepthReached(myDepth);
		}
	}
}
