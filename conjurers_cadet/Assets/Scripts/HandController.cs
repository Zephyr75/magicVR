using UnityEngine;
using UnityEngine.SceneManagement;

public class HandController : MonoBehaviour {

	// Store the hand type to know which button should be pressed
	public enum HandType : int { LeftHand, RightHand };
	[Header( "Hand Properties" )]
	public HandType handType;
	public Transform rightHand, leftHand;


	// Store the player controller to forward it to the object
	[Header( "Player Controller" )]
	public MainPlayerController playerController;
	public Transform trackingSpace, player;


    // Allow to set a cooldown
    [Header( "Magic Cooldown (s)" )]
    public float cooldown = 2.0f;


    // use various sounds
    public AudioSource magicReady;
    public AudioSource magicFailed;
    public AudioSource grabSound;
    public AudioSource shootSound;

    public GameObject magicHand;


	private AudioSource audioSource;
	private Transform far_grasped_object;
	private float far_grasped_distance;
    protected float magic_cooldown;
    [Header("Magic Prefab")]
    public Rigidbody fireballPrefab;
    public Rigidbody iceballPrefab;


    private Rigidbody magicPrefab;


	// Store all gameobjects containing an Anchor
	// N.B. This list is static as it is the same list for all hands controller
	// thus there is no need to duplicate it for each instance
	static protected ObjectAnchor[] anchors_in_the_scene;
	
	// Store the object atached to this hand
	// N.B. This can be extended by using a list to attach several objects at the same time
	static protected ObjectAnchor object_grasped = null;

	void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}
	
	void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
	
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		anchors_in_the_scene = null;

		// Prevent multiple fetch
		if ( anchors_in_the_scene == null ) anchors_in_the_scene = GameObject.FindObjectsOfType<ObjectAnchor>();

		// audioSource = GetComponent<AudioSource>();
	    magic_cooldown = cooldown;
        magicPrefab = iceballPrefab;

		object_grasped = null;

		Debug.LogWarningFormat( "anchors_in_the_scene = {0}", anchors_in_the_scene.Length );
	}


	// This method checks that the hand is closed depending on the hand side
	protected bool is_hand_closed () {
		// Case of a left hand
		if ( handType == HandType.LeftHand ) return OVRInput.Get( OVRInput.Axis1D.PrimaryHandTrigger ) > 0.5;
			// OVRInput.Get( OVRInput.Button.Three )                           // Check that the A button is pressed
			// && OVRInput.Get( OVRInput.Button.Four )                         // Check that the B button is pressed
			// && OVRInput.Get( OVRInput.Axis1D.PrimaryHandTrigger ) > 0.5     // Check that the middle finger is pressing
			// && OVRInput.Get( OVRInput.Axis1D.PrimaryIndexTrigger ) > 0.5;   // Check that the index finger is pressing


		// Case of a right hand
		else return OVRInput.Get( OVRInput.Axis1D.SecondaryHandTrigger ) > 0.5;
			// OVRInput.Get( OVRInput.Button.One )                             // Check that the A button is pressed
			// && OVRInput.Get( OVRInput.Button.Two )                          // Check that the B button is pressed
			// && OVRInput.Get( OVRInput.Axis1D.SecondaryHandTrigger ) > 0.5   // Check that the middle finger is pressing
			// && OVRInput.Get( OVRInput.Axis1D.SecondaryIndexTrigger ) > 0.5; // Check that the index finger is pressing
	}

	// This method checks that the main trigger is pressed depending on the hand side
	protected bool is_trigger_pressed () {
		// Case of a left hand
		if ( handType == HandType.LeftHand ) return OVRInput.Get( OVRInput.Axis1D.PrimaryIndexTrigger ) > 0.5;


		// Case of a right hand
		else return OVRInput.Get( OVRInput.Axis1D.SecondaryIndexTrigger ) > 0.5;
	}

    protected bool is_stick_pressed() {
        if ( handType == HandType.LeftHand ) return OVRInput.Get( OVRInput.Button.PrimaryThumbstick );
        else return OVRInput.Get( OVRInput.Button.SecondaryThumbstick );
    }

	// Automatically called at each frame
	void Update () { handle_controller_behavior(); }


	// Store the previous state of triggers to detect edges
	protected bool is_hand_closed_previous_frame = false;
    protected bool is_magic_pressed_previous_frame = false;
    protected bool is_stick_pressed_previous_frame = false;
    protected bool is_trigger_pressed_previous_frame = false;
    protected Rigidbody fireballInstance;


    // store old transform parent
    protected Transform old_parent = null;


    protected float max_size = 1.0f;
    private bool magic_ready = true;


	private Vector3 get_hand_direction () {
		// Case of a left hand
		if ( handType == HandType.LeftHand ) return leftHand.transform.forward;
		// Case of a right hand
		else return rightHand.transform.forward;
	}

	private Vector3 get_hand_position () {
		// Case of a left hand
		if ( handType == HandType.LeftHand ) return leftHand.transform.position;
		// Case of a right hand
		else return rightHand.transform.position;
	}

    private bool is_magic_pressed() {
        if ( handType == HandType.LeftHand ) return OVRInput.Get( OVRInput.Button.Four );
        else return OVRInput.Get( OVRInput.Button.Two);
    }

    private void vibrate_hand(){
        float amp = 0;
        float freq = 1;

        if (handType == HandType.LeftHand) OVRInput.SetControllerVibration(freq, amp, OVRInput.Controller.LTouch);
        else OVRInput.SetControllerVibration(freq, amp, OVRInput.Controller.RTouch);
    }

	/// <summary>
	/// This method handles the linking of object anchors to this hand controller
	/// </summary>
	protected void handle_controller_behavior () {

		// Check if there is a change in the grasping state (i.e. an edge) otherwise do nothing
		bool hand_closed = is_hand_closed();
        bool magic_pressed = is_magic_pressed();
        bool stick_pressed = is_stick_pressed();

		// Check if there is a change in the grasping state (i.e. an edge) otherwise do nothing
		bool trigger_pressed = is_trigger_pressed();

        magic_cooldown += Time.deltaTime;


        //TODO: play a little sound or make it even spark quickly
        if(magic_cooldown > cooldown && !magic_ready){
            Debug.LogWarningFormat("READY TO SHOOT");
            magic_ready = true;
            // magicReady.Play();
            vibrate_hand();
            GameObject particles = Instantiate(magicHand, this.transform.position, this.transform.rotation);
        }


        if(!hand_closed && !trigger_pressed){


            if(stick_pressed && !is_stick_pressed_previous_frame){
                Debug.LogWarningFormat("SWITCHING MAGIC");
                magicPrefab = magicPrefab == iceballPrefab ? fireballPrefab : iceballPrefab;
                grabSound.Play();
            } else{



                if(magic_pressed && !magic_ready && !is_magic_pressed_previous_frame){
                    magicFailed.Play();
                }
                
                if(magic_ready){
                    if(magic_pressed){
                        if(!is_magic_pressed_previous_frame) {
                                Debug.LogWarningFormat("SHOOTING MAGIC");
                                fireballInstance = Instantiate(magicPrefab, this.transform.position, this.transform.rotation) as Rigidbody;
                                //store old parent
                                old_parent = fireballInstance.transform.parent;
                                fireballInstance.transform.SetParent(this.transform);
                            

                        } else {
                            // increase size of fireball
                            if(fireballInstance.transform.localScale.x < max_size){
                                fireballInstance.transform.localScale += new Vector3(0.001f, 0.001f, 0.001f);
                            }

                        }

                    } else if (is_magic_pressed_previous_frame){
                            fireballInstance.transform.parent = old_parent;
                            fireballInstance.AddForce(this.transform.forward * 1000);
                            // enable explosion script
                            fireballInstance.GetComponent<Explosion>().enabled = true;
                            fireballInstance.GetComponent<SphereCollider>().isTrigger = false;
                            fireballInstance.useGravity = true;
            				magic_cooldown = 0.0f;
                            magic_ready = false;
                            // shootSound.Play();
                    }
                }
           
            }
        }


        
        is_magic_pressed_previous_frame = magic_pressed;
        is_stick_pressed_previous_frame = stick_pressed;

       

		// Make grasped object follow the hand at a fixed distance
		if (far_grasped_object != null) {
			far_grasped_object.position = this.transform.position + get_hand_direction() * far_grasped_distance;
			if ( OVRInput.GetDown( OVRInput.Button.Three ) ) {
				far_grasped_distance -= 1f;
				if (far_grasped_distance < 1f) far_grasped_distance = 1f;
			}
			if ( OVRInput.GetDown( OVRInput.Button.Four ) ) {
				far_grasped_distance += 1f;
			}
		}




		//==============================================//
		// Define the behavior when the trigger is used //
		//==============================================//

        if(trigger_pressed && !is_trigger_pressed_previous_frame && magic_pressed){
            return;
        }

		this.GetComponent<LineRenderer>().enabled = trigger_pressed;


		if ( trigger_pressed) {

			// Log hand action detection
			Debug.LogWarningFormat( "{0} trigger pressed", this.transform.parent.name );

			// Draw line renderer to show the raycast
			LineRenderer line_renderer = this.GetComponent<LineRenderer>();

			// Throw raycast to detect teleportation point
			RaycastHit hit;

			if ( Physics.Raycast( this.transform.position, get_hand_direction(), out hit, 100f) ) {

				// Remote grab
				if (handType == HandType.LeftHand) {
					if (hit.transform.GetComponent<ObjectAnchor>() != null && far_grasped_object == null && hit.transform.gameObject.tag != "Cauldron") {
						far_grasped_object = hit.transform;
						far_grasped_distance = hit.distance;
					}
				}
				// Teleportation
				else {
					if ( OVRInput.GetDown( OVRInput.Button.One ) && hit.transform.tag == "Terrain" ) {
						Debug.LogWarningFormat( "{0} teleported to {1}", this.transform.parent.name, hit.point );
						Vector3 direction = hit.point - player.position;
						direction.y = 0;
						player.position -= direction;
					}
				}

				line_renderer.SetPosition( 0, this.transform.position );
				line_renderer.SetPosition( 1, hit.point );
			}
			else {
				line_renderer.SetPosition( 0, this.transform.position );
				line_renderer.SetPosition( 1, this.transform.position + get_hand_direction() * 100f );
			}


		}
		else {
			far_grasped_object = null;
		}

        is_trigger_pressed_previous_frame = trigger_pressed;


		// Check if there is a change in the grasping state (i.e. an edge) otherwise do nothing
		if ( hand_closed == is_hand_closed_previous_frame) return;
		is_hand_closed_previous_frame = hand_closed;


		//==============================================//
		// Define the behavior when the hand get closed //
		//==============================================//
		if ( hand_closed && !magic_pressed) {

			// Log hand action detection
			Debug.LogWarningFormat( "{0} get closed", this.transform.parent.name );
			Debug.LogWarningFormat( "object_grasped = {0}", object_grasped );

			// Determine which object available is the closest from the left hand
			int best_object_id = -1;
			float best_object_distance = float.MaxValue;
			float object_distance;

			// Iterate over objects to determine if we can interact with it
			for ( int i = 0; i < anchors_in_the_scene.Length; i++ ) {

				// Skip object not available
				if ( !anchors_in_the_scene[i].is_available() ) continue;

				// Skip object requiring special upgrades
				if ( !anchors_in_the_scene[i].can_be_grasped_by( playerController ) ) continue;



				// Compute the distance to the object
				object_distance = Vector3.Distance( this.transform.position, anchors_in_the_scene[i].transform.position );

				// Keep in memory the closest object
				// N.B. We can extend this selection using priorities
				if ( object_distance < best_object_distance && object_distance <= anchors_in_the_scene[i].get_grasping_radius() ) {
					best_object_id = i;
					best_object_distance = object_distance;
				}
			}

			

			// If the best object is in range grab it
			if ( best_object_id != -1 ) {

				// Store in memory the object grasped
				object_grasped = anchors_in_the_scene[best_object_id];

				// Log the grasp
				Debug.LogWarningFormat( "{0} grasped {1}", this.transform.parent.name, object_grasped.name );

				// Grab this object
				object_grasped.attach_to( this );

				// Set rigidbody to kinematic
				object_grasped.GetComponent<Rigidbody>().isKinematic = true;

				// Play the sound
				grabSound.Play();

				// If the object is a sword, align it with the hand
				if (object_grasped.gameObject.tag == "Sword") {
					object_grasped.transform.eulerAngles = get_hand_direction() + new Vector3(0, 0, 90);
					object_grasped.transform.position = get_hand_position() - object_grasped.transform.up * 0.4f;
				}
			}




		//==============================================//
		// Define the behavior when the hand gets opened //
		//==============================================//
		} else if ( object_grasped != null ) {
			// Log the release
			Debug.LogWarningFormat("{0} released {1}", this.transform.parent.name, object_grasped.name );

			// Release the object
			object_grasped.detach_from( this );

			// Set rigidbody to non-kinematic
			object_grasped.GetComponent<Rigidbody>().isKinematic = false;

			// Apply a force to the object
			object_grasped.GetComponent<Rigidbody>().velocity = trackingSpace.transform.TransformVector( OVRInput.GetLocalControllerVelocity( handType == HandType.LeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch ) );
		}



	}
}