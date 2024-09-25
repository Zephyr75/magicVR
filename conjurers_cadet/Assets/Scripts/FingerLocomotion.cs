using UnityEngine;
using static OVRHand;

public class FingerLocomotion : MonoBehaviour {

	[Header( "Hands" )]
	// Bindings with OVR Hands
	public OVRHand leftHand;
	public OVRHand rightHand;

	[Header( "Maximum Distance" )]
	[Range( 2f, 30f )]
	// Store the maximum distance the player can teleport
	public float maximumTeleportationDistance = 15f;

	[Header( "Marker" )]
	// Store the refence to the marker prefab used to highlight the targeted point
	public GameObject markerPrefab;
	protected GameObject marker_prefab_instanciated;


	// Retrieve the character controller used later to move the player in the environment
	protected CharacterController character_controller;
	void Start () { character_controller = this.GetComponent<CharacterController>(); }


	// Store the pointing hand and the non pointing hand
	protected OVRHand pointing_hand;
	protected OVRHand non_pointing_hand;

	// Keep track of the teleportation state to prevent continuous teleportation
	protected bool teleportation_locked = false;


	void Update () {

		// Make sure the pointing hand is still pinched otherwise reset the pointing hand to null
		if ( pointing_hand != null && !pointing_hand.GetFingerIsPinching( HandFinger.Index ) ) pointing_hand = null;

		// If no pointing hand is defined check if one hand is pinching
		if ( pointing_hand == null ) {
			if ( leftHand.GetFingerIsPinching( HandFinger.Index ) ) pointing_hand = leftHand;
			if ( rightHand.GetFingerIsPinching( HandFinger.Index ) ) pointing_hand = rightHand;
		}

		// Store the position of the targeted point
		Vector3 target_point;
		if (
			pointing_hand != null                           // If one hand is pinching
			&& pointing_hand.IsPointerPoseValid             // Skip invalid pointer pose
			&& aim_with( pointing_hand, out target_point )  // The computation of the target position returned a valid point
		) {

			// Instantiate the marker prefab if it doesn't already exists and place it to the targeted position
			if ( marker_prefab_instanciated == null ) marker_prefab_instanciated = GameObject.Instantiate( markerPrefab, this.transform );
			marker_prefab_instanciated.transform.position = target_point;

			// Deduce the non pointing hand
			non_pointing_hand = ( pointing_hand == leftHand ) ? rightHand : leftHand;

			// Check if the other hand is pinching or not
			if ( !non_pointing_hand.GetFingerIsPinching( HandFinger.Index ) ) {
				// Reset the teleportation state
				teleportation_locked = false;
				return;
			}

			// Prevent continuous teleportation
			if ( teleportation_locked ) return;
			teleportation_locked = true;

			// Tell the character controller to move to the teleportation point
			character_controller.Move( target_point - this.transform.position );


		} else {
			// Remove the cursor
        		if ( marker_prefab_instanciated != null ) Destroy( marker_prefab_instanciated );
        		marker_prefab_instanciated = null;

			// Reset the teleportation state
			teleportation_locked = false;
		}

	}

	protected bool aim_with ( OVRHand a_hand, out Vector3 target_point ) {

		// Default the "output" target point to the null vector
		target_point = new Vector3();

		// Launch the ray cast and leave if it doesn't hit anything
		RaycastHit hit;
		if ( !Physics.Raycast( a_hand.transform.position, a_hand.PointerPose.forward, out hit, Mathf.Infinity ) ) return false;

		// If the aimed point is out of range (i.e. the raycast distance is above the maximum distance) then prevent the teleportation
		if ( hit.distance > maximumTeleportationDistance ) return false;

		// "Output" the target point
		target_point = hit.point;
		return true;
	}
}