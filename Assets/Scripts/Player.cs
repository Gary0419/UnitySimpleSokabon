﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sokabon.CommandSystem;
using Sokabon.StateMachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Sokabon
{
	public class Player : StateChangeListener
	{
		public int _gelCount = 0;
		[SerializeField] private GameObject gelPrefab;
		[SerializeField] private LayerSettings layerSettings;

		private Block _block;
		[SerializeField] private TurnManager _turnManager;
		[SerializeField] private BlockManager blockManager;
		private bool _canMove = true;
		public bool IsDead { get; private set; }
		
		private Queue<Vector2Int> _movementQueue;
		private void Awake()
		{
			_movementQueue = new Queue<Vector2Int>();
			_canMove = true;
			_block = GetComponent<Block>();

			var blocks = FindObjectsOfType<Block>();
			foreach (var block in blocks)
			{
				if (block == _block)
				{
					continue;
				}
				
				block.AtNewPositionEvent += CheckForDeath;
			}
			
			//We have a dependency on TurnManager.
			//TurnManager has not implemented the singleton pattern in this example. This is a clear weak link in this project as an example project.
			//Mostly that's because I don't want to demonstrate the singleton pattern here....
			//TurnManager doesn't need to be a monobehaviour. We, the Player, could just have one. turnManager = new TurnManager(); It could also be a ScriptableObject. ScriptableObject-Instead-of-singletons data approach is something I am partial to, but it's got all sorts of quirks, to put it nicely.
			//I like to keep my player pretty bare, and move logic away from them to managers that can just hang out. A) it makes working with AI, game-state-search (solving), or such where we may not have a proper "player" easier, and B) it makes destroying the player for animations and fade-outs and ragdolls and cutscenes and such easier.
			if (_turnManager == null)
			{
				Debug.LogWarning("Player object needs TurnManager set, or TurnManager not found in scene. Searching for one.",gameObject);
				_turnManager = GameObject.FindObjectOfType<TurnManager>();
			}

			if (blockManager == null)
			{
				Debug.LogWarning(
					"Player object needs BlockManager set, or BlockManager not found in scene. Searching for one.",
					gameObject);
				blockManager = FindObjectOfType<BlockManager>();
			}
		}
		
		private void CheckForDeath()
		{
			Collider2D col = Physics2D.OverlapCircle(transform.position, 0.3f, layerSettings.blockLayerMask);
			if (col?.GetComponent<Block>() != null)
			{
				IsDead = true;
			}
		}

		protected override void OnEnterEvent()
		{
			_canMove = true;
		}

		protected override void OnExitEvent()
		{
			_canMove = false;
		}

		private void Update()
		{
			if (!_canMove)
			{
				return;//cant move.
			}

			//Todo: Joystick support. Switch to new input system.
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			{
				_movementQueue.Enqueue(Vector2Int.up);
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			{
				_movementQueue.Enqueue(Vector2Int.down);
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			{
				_movementQueue.Enqueue(Vector2Int.left);
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			{
				_movementQueue.Enqueue(Vector2Int.right);
			}
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				_movementQueue.Enqueue(Vector2Int.zero);
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				if (!_block.IsAnimating && _gelCount > 0)
				{
					_gelCount--;
					Instantiate(gelPrefab, transform.position, Quaternion.identity);
				}
			}
			else if (Input.GetKeyDown(KeyCode.Z))
			{
				_turnManager.Undo();
			}

			if (!_block.IsAnimating && _movementQueue.Count > 0)
			{
				Vector2Int dir = _movementQueue.Dequeue();
				if (!blockManager.PlayerTryMove(_block, dir))
				{
					_movementQueue.Clear();
				}
			}
			
		}
	}
}
