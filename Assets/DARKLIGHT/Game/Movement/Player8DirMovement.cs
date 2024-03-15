using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.Game.Movement
{
	[RequireComponent(typeof(Rigidbody))]
	public class Player8DirMovement : MonoBehaviour
	{
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
