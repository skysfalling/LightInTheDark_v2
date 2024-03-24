namespace Darklight.Player.Movement
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Darklight.UnityExt.Input;

	public enum PlayerState
	{
		IDLE, MOVING, THROWING,
		STUNNED, GRABBED, PANIC,
		DASH, SLOWED, INACTIVE
	}

	[RequireComponent(typeof(Rigidbody))]
	public class PlayerMovement : MonoBehaviour
	{
		//GameManager gameManager;
		Rigidbody rb;
		Animator animator;
		//PlayerInventory inventory;
		UniversalInputManager inputManager;


		public PlayerState state = PlayerState.IDLE;
		public bool inputIsDown;

		[Space(10)]
		public float speed = 10;
		private float defaultSpeed;
		public float maxVelocity = 10;
		public Vector3 moveTarget;
		public float distToTarget;

		[Space(10)]
		public Vector3 moveDirection;

		[Header("Slowed Values")]
		public float slowedSpeed;
		public float slowedTimer;

		[Header("Dash Values")]
		public float dashSpeed;
		public float dashDuration;
		private float dashTimer;
		public float dashDelay;
		public bool dashReady = true;


		[Header("Throw Ability")]
		public Transform aimIndicator;
		public Transform throwParent;
		public GameObject throwObject;
		public float throwingMoveSpeed = 10;
		public float throwForce = 20;
		public bool inThrow;
		[Space(5)]
		public string throwSortingLayer = "Player";
		public int throwSortingOrder = 5;


		[Header("Charge Flash")]
		public float chargeFlashActivateDuration = 1.5f;
		public bool chargeDisabled;
		public float chargeDisableTime = 1;
		public float chargeLightIntensity = 3;


		[Header("Interaction")]
		public float interactionRange = 5;


		[Header("Grabbed")]
		//GrabHandAI grabbedHand;
		public int struggleCount;



		// Start is called before the first frame update
		void Start()
		{
			//gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
			rb = GetComponent<Rigidbody>();
			//animator = GetComponent<PlayerAnimator>();
			//inventory = GetComponent<PlayerInventory>();
			inputManager = FindFirstObjectByType<UniversalInputManager>();


			moveTarget = transform.position;

			defaultSpeed = speed;
		}

		// Update is called once per frame
		void Update()
		{
			Inputs();
		}

		private void FixedUpdate()
		{
			StateMachine();
		}

		public void Inputs()
		{
			// << BASIC MOVE >>
			// set move target
			Vector2 moveInput = inputManager.moveInput.ReadValue<Vector2>();
			moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

			// if move input down , move
			if (state == PlayerState.IDLE || state == PlayerState.MOVING)
			{
				if (moveDirection != Vector3.zero)
				{
					state = PlayerState.MOVING;
				}
				else
				{
					state = PlayerState.IDLE;
				}
			}

			/*
			// << THROW ACTION DOWN >>
			inputManager.aAction.started += ctx =>
			{
				// << GET THROW OBJECT >>
				if (throwObject == null && !inThrow)
				{
					NewThrowObject();
					state = PlayerState.THROWING;
				}
			};

			// << THROW ACTION UP >>
			inputManager.aAction.canceled += ctx =>
			{
				// << THROW OBJECT >>
				if (throwObject != null)
				{
					ThrowObject();
				}

				state = PlayerState.IDLE;
				aimIndicator.gameObject.SetActive(false);

			};

			// << MOVE ACTION >>
			inputManager.bAction.started += ctx =>
			{
				// << STRUGGLE >>
				if (state == PlayerState.GRABBED)
				{
					inventory.DropAllItems();
					gameManager.camManager.ShakeCamera();

					struggleCount++;
				}
				// << DASH >>
				else if (state != PlayerState.GRABBED)
				{
					struggleCount = 0;

					// << START DASH >>
					if ( state == PlayerState.IDLE
						|| state == PlayerState.MOVING
						|| state == PlayerState.THROWING ) { Dash(); }
				}
			};
			*/
		}

		public void StateMachine()
		{

			// << DISABLE COLLIDER >>
			//if (state == PlayerState.GRABBED || state == PlayerState.INACTIVE) { EnableCollider(false); }
			//else { EnableCollider(true); }


			switch (state)
			{
				case PlayerState.IDLE:
					rb.velocity = Vector3.ClampMagnitude(rb.velocity, 0);
					break;

				case PlayerState.MOVING:
					rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
					rb.velocity = moveDirection * speed;

					break;

				case PlayerState.DASH:
					if (dashTimer > 0)
					{
						dashTimer -= Time.deltaTime;
						rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
						rb.velocity = moveDirection * dashSpeed;
					}
					else
					{
						state = PlayerState.IDLE;
					}
					break;

				case PlayerState.SLOWED:
				case PlayerState.PANIC:
					if (slowedTimer > 0)
					{
						slowedTimer -= Time.deltaTime;
						rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
						rb.velocity = moveDirection * slowedSpeed;
					}
					else
					{
						state = PlayerState.IDLE;
					}
					break;

				case PlayerState.GRABBED:
					break;


				case PlayerState.THROWING:

					// move player at throw move speed
					rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
					rb.velocity = moveDirection * throwingMoveSpeed;

					// throw object move to parent
					if (throwObject != null)
					{

						/*
						// if not thrown yet, move object towards throw parent
						if (throwObject.GetComponent<Item>().state == ItemState.PLAYER_INVENTORY)
						{
							Vector3 newDirection = Vector3.MoveTowards(throwObject.transform.position, throwParent.transform.position, inventory.circleSpeed * Time.deltaTime);
							throwObject.transform.position = newDirection;
						}
						*/
					}
					break;

				default:
					rb.velocity = Vector3.zero;
					break;
			}

		}
		/*
			#region << THROW OBJECT >>
			public void NewThrowObject()
			{
				if (inventory.inventory.Count > 0)
				{
					throwObject = inventory.RemoveItemToThrow(inventory.inventory[0]); // remove 0 index item
					throwObject.transform.parent = throwParent;

					throwObject.GetComponent<Item>().SetSortingOrder(throwSortingOrder, throwSortingLayer);

				}
			}

			public void ThrowObject()
			{
				if (throwObject != null && !inThrow)
				{
					throwObject.transform.parent = null;

					StartCoroutine(ThrowObject(throwObject, moveDirection, throwForce));

				}
			}

			public IEnumerator ThrowObject(GameObject obj, Vector2 direction, float force)
			{
				inThrow = true;

				Debug.Log("Throw " + obj.name);
				obj.GetComponent<Item>().state = ItemState.THROWN;
				obj.GetComponent<Item>().ResetSortingOrder();

				// remove from inventory and set state
				inventory.RemoveItemToThrow(obj);
				obj.transform.parent = null;

				obj.GetComponent<Item>().SetSortingOrder(throwSortingOrder, throwSortingLayer);
				obj.GetComponent<Rigidbody2D>().AddForce(direction * force, ForceMode2D.Impulse);

				yield return new WaitForSeconds(0.5f); // wait for item to get out of player's range -> to stop immediate pickup 

				throwObject = null; // set throw object to null


				inThrow = false;
			}
			#endregion

			#region<< STUNNED >>
			public void Stunned(float time)
			{
				StartCoroutine(StunCoroutine(time));
			}

			public IEnumerator StunCoroutine(float time)
			{
				state = PlayerState.STUNNED;

				yield return new WaitForSeconds(time);

				state = PlayerState.IDLE;
			}
			#endregion

			#region << SET STATES >>

			public void Inactive()
			{
				state = PlayerState.INACTIVE;
			}

			public void Idle()
			{
				state = PlayerState.IDLE;
			}

			public void Grabbed()
			{
				state = PlayerState.GRABBED;
			}

			public void Stunned()
			{
				state = PlayerState.STUNNED;
			}

			public void Moving()
			{
				state = PlayerState.MOVING;
			}

			public void Panic(float time)
			{
				slowedTimer = time;
				state = PlayerState.PANIC;
			}

			public void Slowed(float time)
			{
				slowedTimer = time;
				state = PlayerState.SLOWED;
			}

			public void Dash()
			{
				if (dashReady)
				{
					dashTimer = dashDuration;
					state = PlayerState.DASH;

					animator.PlayDashEffect();

					StartCoroutine(DashDelay(dashDelay));
				}

			}

			IEnumerator DashDelay(float timer)
			{
				dashReady = false;

				yield return new WaitForSeconds(timer);

				dashReady = true;
			}

			#endregion


			public void EnableCollider(bool enabled)
			{
				GetComponent<CapsuleCollider>().enabled = enabled;
			}

			public void OnDrawGizmos()
			{
				//Gizmos.color = Color.white;
				//Gizmos.DrawWireSphere(transform.position, interactionRange);
			}
			*/
	}


}