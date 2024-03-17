using System.Collections;
using System.Collections.Generic;
using Darklight.UniversalInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight.Game.Movement
{
	[RequireComponent(typeof(Rigidbody))]
	public class Player8DirMovement : MonoBehaviour
	{
		private UniversalInputManager _universalInputManager;
		void Awake()
		{
			_universalInputManager = UniversalInputManager.Instance;
			InputAction moveInput = _universalInputManager.moveInput;
			moveInput.performed += context => UpdateMoveDirection(moveInput.ReadValue<Vector2>()); // Send out move input value
			moveInput.canceled += context => UpdateMoveDirection(Vector2.zero); // Reset input value
		}


		/// <summary>
		/// private storage of the current input direction
		/// </summary>
		[SerializeField] private Vector2 _universalMoveDirection;
		[SerializeField] private int _multiplier = 10;

		/// <summary>
		/// Gets the child Rigidbody component
		/// </summary> <summary>
		/// 
		/// </summary>
		/// <typeparam name="Rigidbody"></typeparam>
		/// <returns></returns>
		public new Rigidbody rigidbody => GetComponent<Rigidbody>();


		/// <summary>
		/// This is updated by the UniversalInputManager
		/// </summary>
		/// <param name="moveInput"></param> <summary>
		/// 
		/// </summary>
		/// <param name="moveInput"></param>
		public void UpdateMoveDirection(Vector2 moveInput)
		{
			this._universalMoveDirection = moveInput;
		}

		public void FixedUpdate()
		{
			this.rigidbody.velocity = new Vector3(this._universalMoveDirection.x * this._multiplier, 0, this._universalMoveDirection.y * this._multiplier);
		}
	}
}
