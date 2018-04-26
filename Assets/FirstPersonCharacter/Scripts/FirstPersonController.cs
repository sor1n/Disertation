using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using System.Xml.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;

[RequireComponent (typeof(CharacterController))]
[RequireComponent (typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour
{
	[SerializeField] private bool m_IsWalking;
	[SerializeField] private float m_WalkSpeed;
	[SerializeField] private float m_RunSpeed;
	[SerializeField] [Range (0f, 1f)] private float m_RunstepLenghten;
	[SerializeField] private float m_JumpSpeed;
	[SerializeField] private bool m_invertedControls;
	[SerializeField] private float m_StickToGroundForce;
	[SerializeField] private float m_GravityMultiplier;
	[SerializeField] private MouseLook m_MouseLook;
	[SerializeField] private bool m_UseFovKick;
	[SerializeField] private FOVKick m_FovKick = new FOVKick ();
	[SerializeField] private bool m_UseHeadBob;
	[SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob ();
	[SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob ();
	[SerializeField] private float m_StepInterval;
	[SerializeField] private AudioClip[] m_FootstepSounds;
	// an array of footstep sounds that will be randomly selected from.
	[SerializeField] private AudioClip m_JumpSound;
	// the sound played when character leaves the ground.
	[SerializeField] private AudioClip m_LandSound;
	// the sound played when character touches back on ground.

	private Camera m_Camera;
	private bool m_Jump, m_Crouch;
	private float m_YRotation;
	private Vector2 m_Input;
	private Vector3 m_MoveDir = Vector3.zero;
	private CharacterController m_CharacterController;
	private CollisionFlags m_CollisionFlags;
	private bool m_PreviouslyGrounded;
	private Vector3 m_OriginalCameraPosition;
	private float m_StepCycle;
	private float m_NextStep;
	private bool m_Jumping;
	private AudioSource m_AudioSource;
	private int door_mask;

	private int maxRun = 200, runMeter;
	private bool adjustedSpeed = false, moveInLocker = false, canContinue = false, interract = false, startInterraction = false, isCreative = false, hiding = false, exhausted = false;
	Transform targetLocker = null;

	// Use this for initialization
	private void Start ()
	{
		m_CharacterController = GetComponent<CharacterController> ();
		m_Camera = Camera.main;
		m_OriginalCameraPosition = m_Camera.transform.localPosition;
		m_FovKick.Setup (m_Camera);
		m_HeadBob.Setup (m_Camera, m_StepInterval);
		m_StepCycle = 0f;
		m_NextStep = m_StepCycle / 2f;
		m_Jumping = false;
		m_AudioSource = GetComponent<AudioSource> ();
		m_MouseLook.Init (transform, m_Camera.transform);
		door_mask = LayerMask.GetMask ("Door");
		runMeter = maxRun;
	}

	// Update is called once per frame
	private void Update ()
	{
		if (GetComponent<CharacterController> ().enabled) {
			if (m_invertedControls) m_MouseLook.invertedControls = -1;
			else m_MouseLook.invertedControls = 1;
			RotateView ();
			// the jump state needs to read here to make sure it is not missed
			if (!m_Jump) m_Jump = CrossPlatformInputManager.GetButton ("Jump");
			if (!m_Crouch) m_Crouch = CrossPlatformInputManager.GetButton ("Crouch");

			if (!m_PreviouslyGrounded && m_CharacterController.isGrounded) {
				StartCoroutine (m_JumpBob.DoBobCycle ());
				PlayLandingSound ();
				m_MoveDir.y = 0f;
				m_Jumping = false;
			}
			if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded) {
				m_MoveDir.y = 0f;
			}

			m_PreviouslyGrounded = m_CharacterController.isGrounded;
		}

		// Door opening
		if (Input.GetButtonDown ("Interact")) {
			RaycastHit hit;
			Physics.Raycast (transform.position, transform.forward, out hit, 3f, door_mask);
			Transform obj = hit.transform;
			if (obj != null) {
				Animator anim = obj.GetComponent<Animator> ();
				if (Vector3.Distance (transform.position, obj.position) < 2) {
					targetLocker = obj;
					if (obj.name.Contains ("Door")) anim.SetBool ("open", !anim.GetBool ("open"));
					else if (obj.name.Contains ("Locker") && !interract) {
						interract = true;
						startInterraction = true;					
					}
				}
			}
		}

		// Set to Creative & generate a new map
		if (Input.GetKeyDown (KeyCode.F3)) isCreative = !isCreative;
		if (Input.GetKeyDown (KeyCode.F4)) SceneManager.LoadScene (1);

		// Moving into locker animation
		if (interract) {
			if (startInterraction && !hiding) {
				targetLocker.GetComponent<Animator> ().SetBool ("open", true);

				transform.position = targetLocker.GetChild (2).position;
				transform.LookAt (targetLocker.GetChild (3));

				gameObject.layer = LayerMask.NameToLayer ("Incorporeal");
				transform.GetComponent <CharacterController> ().enabled = false;

				moveInLocker = true;
				startInterraction = false;
			}
			if (moveInLocker && !hiding) {
				StartCoroutine ("Wait", 1.0f);

				Vector3 targetPos = targetLocker.GetChild (3).position;

				if (canContinue) transform.position = Vector3.MoveTowards (transform.position, targetPos, m_WalkSpeed * Time.deltaTime);
				if (Vector3.Distance (transform.position, targetPos) < 0.1f) {
					gameObject.layer = LayerMask.NameToLayer ("Default");
					targetLocker.GetComponent<Animator> ().SetBool ("open", false);
					transform.GetComponent <CharacterController> ().enabled = true;

					transform.GetChild (0).GetChild (0).GetComponent<Light> ().intensity = 1;
					moveInLocker = false;
					hiding = true;
					canContinue = false;
					interract = false;
				}
			} else if (hiding) {
				targetLocker.GetComponent<Animator> ().SetBool ("open", true);
				transform.LookAt (targetLocker.GetChild (2));

				gameObject.layer = LayerMask.NameToLayer ("Incorporeal");
				transform.GetComponent <CharacterController> ().enabled = false;

				StartCoroutine ("Wait", 1.0f);

				Vector3 targetPos = targetLocker.GetChild (2).position;
				if (canContinue) transform.position = Vector3.MoveTowards (transform.position, targetPos, m_WalkSpeed * Time.deltaTime);
				if (Vector3.Distance (transform.position, targetPos) < 0.1f) {
					gameObject.layer = LayerMask.NameToLayer ("Default");
					targetLocker.GetComponent<Animator> ().SetBool ("open", false);
					transform.GetComponent <CharacterController> ().enabled = true;

					transform.GetChild (0).GetChild (0).GetComponent<Light> ().intensity = 3;
					hiding = false;
					canContinue = false;
					interract = false;
				}
			}
		}
	}

	IEnumerator Wait (float t)
	{
		yield return new WaitForSeconds (t);
		canContinue = true;
	}

	private void PlayLandingSound ()
	{
		m_AudioSource.clip = m_LandSound;
		m_AudioSource.Play ();
		m_NextStep = m_StepCycle + .5f;
	}


	private void FixedUpdate ()
	{
		if (GetComponent<CharacterController> ().enabled) {
			float speed;
			GetInput (out speed);

			if (speed >= m_RunSpeed && runMeter > 0) runMeter--;
			else if (runMeter < maxRun) runMeter++;

			if (runMeter <= 0) exhausted = true;
			else if (runMeter >= maxRun) exhausted = false;

			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

			// get a normal for the surface that is being touched to move along it
			RaycastHit hitInfo;
			Physics.SphereCast (transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
			                    m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane (desiredMove, hitInfo.normal).normalized;

			m_MoveDir.x = desiredMove.x * speed;
			m_MoveDir.z = desiredMove.z * speed;

			if (m_CharacterController.isGrounded) {
				m_MoveDir.y = -m_StickToGroundForce;
				if (m_Jump) {
					m_MoveDir.y = m_JumpSpeed;
					PlayJumpSound ();
					m_Jump = false;
					m_Jumping = true;
				}
			} else {
				m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
			}
			if (gameObject.layer == LayerMask.NameToLayer ("Incorporeal")) m_MoveDir.y = 0;
			if (isCreative) {
				if (m_Jump) m_MoveDir.y = m_JumpSpeed;
				if (m_Crouch) m_MoveDir.y = -m_JumpSpeed;
				m_Jump = false;
				m_Crouch = false;
				if (!adjustedSpeed) {
					AdjustSpeed (15, true);
					gameObject.layer = LayerMask.NameToLayer ("Incorporeal");
					RenderSettings.ambientLight = Color.white;
					RenderSettings.fog = false;
					gameObject.transform.GetChild (0).GetComponent<Camera> ().farClipPlane = 1000.0f;
					gameObject.transform.GetChild (0).GetChild (0).gameObject.SetActive (false);
				}
			} else if (adjustedSpeed) {
				AdjustSpeed (-15, false);
				gameObject.layer = LayerMask.NameToLayer ("Player");
				RenderSettings.ambientLight = Color.black;
				RenderSettings.fog = true;
				gameObject.transform.GetChild (0).GetComponent<Camera> ().farClipPlane = 50.0f;
				gameObject.transform.GetChild (0).GetChild (0).gameObject.SetActive (true);
			}
			m_CollisionFlags = m_CharacterController.Move (m_MoveDir * Time.fixedDeltaTime);

			ProgressStepCycle (speed);
			UpdateCameraPosition (speed);

			m_MouseLook.UpdateCursorLock ();
		}
	}

	private void AdjustSpeed (float val, bool changed)
	{
		m_WalkSpeed += val;
		m_RunSpeed += val;
		m_JumpSpeed += val;
		adjustedSpeed = changed;
	}

	private void PlayJumpSound ()
	{
		m_AudioSource.clip = m_JumpSound;
		m_AudioSource.Play ();
	}


	private void ProgressStepCycle (float speed)
	{
		if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0)) {
			m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
			Time.fixedDeltaTime;
		}

		if (!(m_StepCycle > m_NextStep)) {
			return;
		}

		m_NextStep = m_StepCycle + m_StepInterval;

		PlayFootStepAudio ();
	}


	private void PlayFootStepAudio ()
	{
		if (!m_CharacterController.isGrounded) {
			return;
		}
		// pick & play a random footstep sound from the array,
		// excluding sound at index 0
		int n = Random.Range (1, m_FootstepSounds.Length);
		m_AudioSource.clip = m_FootstepSounds[n];
		m_AudioSource.PlayOneShot (m_AudioSource.clip);
		// move picked sound to index 0 so it's not picked next time
		m_FootstepSounds[n] = m_FootstepSounds[0];
		m_FootstepSounds[0] = m_AudioSource.clip;
	}

	private void UpdateCameraPosition (float speed)
	{
		Vector3 newCameraPosition;
		if (!m_UseHeadBob) {
			return;
		}
		if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded) {
			m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob (m_CharacterController.velocity.magnitude +
			(speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
			newCameraPosition = m_Camera.transform.localPosition;
			newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset ();
		} else {
			newCameraPosition = m_Camera.transform.localPosition;
			newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset ();
		}
		m_Camera.transform.localPosition = newCameraPosition;
	}


	private void GetInput (out float speed)
	{
		// Read input
		float horizontal = CrossPlatformInputManager.GetAxis ("Horizontal");
		float vertical = CrossPlatformInputManager.GetAxis ("Vertical");

		bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
		// On standalone builds, walk/run speed is modified by a key press.
		// keep track of whether or not the character is walking or running
		m_IsWalking = !Input.GetKey (KeyCode.LeftShift);
#endif
		// set the desired speed to be walking or running
		speed = m_IsWalking ? m_WalkSpeed : exhausted ? m_WalkSpeed : m_RunSpeed;
		m_Input = new Vector2 (horizontal, vertical);

		// normalize input if it exceeds 1 in combined length:
		if (m_Input.sqrMagnitude > 1) {
			m_Input.Normalize ();
		}

		// handle speed change to give an fov kick
		// only if the player is going to a run, is running and the fovkick is to be used
		if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0) {
			StopAllCoroutines ();
			StartCoroutine (!m_IsWalking ? m_FovKick.FOVKickUp () : m_FovKick.FOVKickDown ());
		}
	}


	private void RotateView ()
	{
		m_MouseLook.LookRotation (transform, m_Camera.transform);
	}


	private void OnControllerColliderHit (ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;
		//dont move the rigidbody if the character is on top of it
		if (m_CollisionFlags == CollisionFlags.Below) {
			return;
		}

		if (body == null || body.isKinematic) {
			return;
		}
		body.AddForceAtPosition (m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
	}

	public bool IsHiding ()
	{
		return hiding;
	}

	public bool IsCreative ()
	{
		return isCreative;
	}
}
