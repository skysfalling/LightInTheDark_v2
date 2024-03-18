using System.Collections;
using System.Collections.Generic;
using Darklight.UniversalInput;
using UnityEngine;
using UnityEngine.InputSystem;
using Darklight.World.Map;
using Darklight.World;
using Darklight.World.Generation;
using System;

namespace Darklight.Game.Movement
{
	[RequireComponent(typeof(Rigidbody))]
	public class Player8DirMovement : MonoBehaviour
	{
		private UniversalInputManager _universalInputManager;
		[SerializeField] public WorldDirection? currentDirection;
		public Vector3 targetPosition;
		[SerializeField] private Vector2 _universalMoveInput;
		public float multiplier = 10;

		/// <summary>
		/// Gets the child Rigidbody component
		/// </summary> <summary>
		/// 
		/// </summary>
		/// <typeparam name="Rigidbody"></typeparam>
		/// <returns></returns>
		public new Rigidbody rigidbody => GetComponent<Rigidbody>();

		void Awake()
		{
			_universalInputManager = UniversalInputManager.Instance;
			InputAction moveInput = _universalInputManager.moveInput;
			moveInput.performed += context => UpdateMoveDirection(moveInput.ReadValue<Vector2>()); // Send out move input value
			moveInput.canceled += context => UpdateMoveDirection(Vector2.zero); // Reset input value
		}

		/// <summary>
		/// This is updated by the UniversalInputManager
		/// </summary>
		/// <param name="moveInput"></param> <summary>
		/// 
		/// </summary>
		/// <param name="moveInput"></param>
		public void UpdateMoveDirection(Vector2 moveInput)
		{
			// store input
			this._universalMoveInput = moveInput.normalized;
			currentDirection = CoordinateMap.GetEnumFromDirectionVector(new Vector2Int((int)_universalMoveInput.x, (int)_universalMoveInput.y));// get private world direction
		}


		public void FixedUpdate()
		{
			//Vector3 targetDirection = targetPosition - transform.position;
			//rigidbody.velocity = targetDirection * _multiplier;

			//transform.position = targetPosition;
			//transform.position = new Vector3(transform.position.x, targetPosition.y, transform.position.z);
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * multiplier);
		}

	}
}
