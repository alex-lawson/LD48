using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingDungeonGenerator : MonoBehaviour
{
	public Transform StartConnector;
	public List<GameObject> HallRooms;
	public List<GameObject> BranchRooms;
	public List<GameObject> OneWayRooms;
	public List<GameObject> DeadEndRooms;

	public int GroupCountMin;
	public int GroupCountMax;

	private List<GameObject> lastRoomGroup = new List<GameObject>();
	private List<GameObject> currentRoomGroup = new List<GameObject>();
	private List<GameObject> nextRoomGroup = new List<GameObject>();

	private Transform lastOpenConnector;

	private int generatedDepth;
	private int reachedDepth;
	private int triggerDepth;
	private int nextTriggerDepth;

	// generator should place:
	//  - some # of hall and branch rooms
	//  - dead end rooms on branches exceeding branch factor only
	//  - a one-way room

	public void OnDepthReached(int newDepth)
	{
		if (newDepth > reachedDepth)
		{
			Debug.Log($"reached new depth {newDepth} from previous {reachedDepth}");

			reachedDepth = newDepth;

			if (reachedDepth >= triggerDepth)
			{
				CleanupAndCycleGroups();
				GenerateNextGroup();
			}
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		GenerateInitialGroups();
	}

	private void GenerateInitialGroups()
	{
		lastOpenConnector = StartConnector;

		GenerateNextGroup();

		currentRoomGroup = nextRoomGroup;

		triggerDepth = nextTriggerDepth;

		Debug.Log($"initial trigger depth is {triggerDepth}");

		nextRoomGroup = new List<GameObject>();

		GenerateNextGroup();
	}

	private void CleanupAndCycleGroups()
	{
		Debug.Log("cleaning up and cycling room groups");

		foreach (GameObject roomObject in lastRoomGroup)
		{
			Destroy(roomObject);
		}

		lastRoomGroup = new List<GameObject>();

		lastRoomGroup = currentRoomGroup;

		currentRoomGroup = nextRoomGroup;

		triggerDepth = nextTriggerDepth;

		Debug.Log($"new trigger depth is {triggerDepth}");

		nextRoomGroup = new List<GameObject>();
	}

	private void GenerateNextGroup()
	{
		nextTriggerDepth = generatedDepth + 1;

		int groupCount = Random.Range(GroupCountMin, GroupCountMax + 1);
		bool needsOneWay = true;
		bool canBranch = true;

		Debug.Log($"generating next room group with {groupCount} rooms and trigger depth {nextTriggerDepth}");

		while (groupCount > 0)
		{
			groupCount--;

			GameObject roomPrefab = null;

			if (needsOneWay)
			{
				roomPrefab = OneWayRooms.RandomElement();

				needsOneWay = false;
			}
			else if (canBranch && Random.value < 0.5f)
			{
				roomPrefab = BranchRooms.RandomElement();

				canBranch = false;
			}
			else
			{
				roomPrefab = HallRooms.RandomElement();
			}

			RollingDungeonRoom thisRoom = PlaceRoom(lastOpenConnector, roomPrefab);

			nextRoomGroup.Add(thisRoom.gameObject);
			generatedDepth++;

			if (thisRoom.TriggerZone)
			{
				thisRoom.TriggerZone.LinkGenerator(this, generatedDepth);
			}

			while (thisRoom.OutConnectors.Count > 1)
			{
				int deadEndex = Random.Range(0, thisRoom.OutConnectors.Count);

				GameObject deadEndPrefab = DeadEndRooms.RandomElement();
				PlaceRoom(thisRoom.OutConnectors[deadEndex], deadEndPrefab);
				thisRoom.OutConnectors.RemoveAt(deadEndex);
			}

			if (thisRoom.OutConnectors.Count > 0)
			{
				lastOpenConnector = thisRoom.OutConnectors[0];
			}
		}
	}

	private RollingDungeonRoom PlaceRoom(Transform outboundConnector, GameObject roomPrefab)
	{
		RollingDungeonRoom prefabRoom = roomPrefab.GetComponent<RollingDungeonRoom>();

		Transform prefabIC = prefabRoom.InConnector;

		Quaternion targetOrientation = outboundConnector.transform.rotation * Quaternion.Inverse(prefabIC.localRotation);
		Vector3 targetPosition = outboundConnector.transform.position - targetOrientation * prefabIC.localPosition;

		RollingDungeonRoom placedRoom = Instantiate(prefabRoom, transform).GetComponent<RollingDungeonRoom>();

		placedRoom.transform.localRotation = targetOrientation;
		placedRoom.transform.position = targetPosition;

		return placedRoom;
	}

	//private bool TryPlacePart(DungeonPart partPrefab, DungeonConnector outboundConnector)
	//{
	//	Quaternion targetOrientation = new Quaternion();
	//	Vector3 targetPosition = Vector3.zero;
	//	int? inboundConnectorId = null;

	//	var prefabICs = partPrefab.InboundConnectorsFor(outboundConnector);
	//	prefabICs.Shuffle();

	//	foreach (var prefabIC in prefabICs)
	//	{
	//		// calculate prospective orientation and position for part instance
	//		targetOrientation = Quaternion.AngleAxis(180, outboundConnector.transform.up) * outboundConnector.transform.rotation * Quaternion.Inverse(prefabIC.transform.localRotation);
	//		targetPosition = outboundConnector.transform.position - targetOrientation * prefabIC.transform.localPosition;

	//		bool canPlace = partPrefab.SkipBoundsCheck || partPrefab.BoundsCheck(outboundConnector.transform, prefabIC.ConnectorId);

	//		if (canPlace)
	//		{
	//			inboundConnectorId = prefabIC.ConnectorId;
	//			break;
	//		}
	//		else
	//		{
	//			//Debug.Log($"Bounds check failed! Can't connect {outboundConnector.gameObject.name} (outbound) to {partPrefab.PartName} {prefabIC.gameObject.name} (inbound)");
	//		}
	//	}

	//	if (!inboundConnectorId.HasValue)
	//		return false;

	//	var partInstance = Instantiate(partPrefab.gameObject, transform).GetComponent<DungeonPart>();
	//	currentDungeonPartInstances.Add(partInstance);

	//	var inboundConnector = partInstance.GetConnector(inboundConnectorId.Value);

	//	// random rotation added separately from bounds check so that bounds check is deterministic
	//	if (inboundConnector.RandomRotation || outboundConnector.RandomRotation)
	//	{
	//		float rotateDegrees = Random.Range(0, 360);
	//		targetOrientation = Quaternion.AngleAxis(180, outboundConnector.transform.up)
	//				* Quaternion.AngleAxis(rotateDegrees, outboundConnector.transform.forward)
	//				* outboundConnector.transform.rotation
	//				* Quaternion.Inverse(inboundConnector.transform.localRotation);
	//		targetPosition = outboundConnector.transform.position - targetOrientation * inboundConnector.transform.localPosition;
	//	}

	//	partInstance.transform.localRotation = targetOrientation;
	//	partInstance.transform.position = targetPosition;

	//	//Debug.Log($"Connected {outboundConnector.gameObject.name} (outbound) to {partPrefab.PartName} {inboundConnector.gameObject.name} (inbound)");

	//	var nextOutboundConnectors = partInstance.OutboundConnectors();
	//	nextOutboundConnectors.Remove(inboundConnector);
	//	currentPhaseStatus.OpenOutbound.Remove(outboundConnector);

	//	// check whether this part makes additional connections to other open connectors
	//	for (var i = nextOutboundConnectors.Count - 1; i > 0; i--)
	//	{
	//		var connA = nextOutboundConnectors[i];
	//		for (var j = currentPhaseStatus.OpenOutbound.Count - 1; j > 0; j--)
	//		{
	//			var connB = currentPhaseStatus.OpenOutbound[j];
	//			if (connA.CanConnectTo(connB))
	//			{
	//				if ((connA.transform.position - connB.transform.position).sqrMagnitude < 0.01f)
	//				{
	//					nextOutboundConnectors.RemoveAt(i);
	//					currentPhaseStatus.OpenOutbound.RemoveAt(j);
	//					//Debug.Log($"Detected additional connection at {connA.transform.position} between {connA.gameObject.name} and {connB.gameObject.name}, closing both.");
	//				}
	//			}
	//		}
	//	}

	//	currentPhaseStatus.OpenOutbound.AddRange(nextOutboundConnectors);

	//	return true;
	//}
}
