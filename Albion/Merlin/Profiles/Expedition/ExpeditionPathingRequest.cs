using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using UnityEngine;

namespace Merlin.Profiles.Expedition {
	class ExpeditionPathingRequest {

		private bool _useCollider;

		private LocalPlayerCharacterView _player;

		private List<Vector3> _path;
		private List<Vector3> _fleePoints;

		private StateMachine<State, Trigger> _state;
		private Vector3 _target;

		public bool IsRunning => _state.State != State.Finish;

		public ExpeditionPathingRequest(LocalPlayerCharacterView player, Vector3 target, List<Vector3> path, bool useCollider = true) {
			_player = player;
			_target = target;
			_path = path;
			_useCollider = useCollider;

			_fleePoints = new List<Vector3>();

			_state = new StateMachine<State, Trigger>(State.Start);
			_state.Configure(State.Start)
				.Permit(Trigger.ApprachTarget, State.Running)
				.Permit(Trigger.ReachedTarget, State.Finish);
			_state.Configure(State.Running)
				.Permit(Trigger.ReachedTarget, State.Start);
		}

		public void Continue() {
			switch (_state.State) {
				case State.Start:
					if (_path.Count > 0)
						_state.Fire(Trigger.ApprachTarget);
					else
						_state.Fire(Trigger.ReachedTarget);

					break;

				case State.Running:
					var currentNode = _path[0];
					var minimumDistance = 3f;

					if (_path.Count < 2 && _useCollider) {
						minimumDistance = 1.0f + _player.GetColliderExtents();

						var directionToPlayer = (_player.transform.position - _target).normalized;
						var bufferDistance = directionToPlayer * minimumDistance;

						currentNode = _target + bufferDistance;
					}

					var distanceToNode = (_player.transform.position - currentNode).sqrMagnitude;

					if (distanceToNode < minimumDistance) {
						_fleePoints.Add(currentNode);
						_path.RemoveAt(0);
					} else {
						_player.RequestMove(currentNode);
					}

					_state.Fire(Trigger.ReachedTarget);
					break;
			}
		}

		// Reverses Continue, to follow the points backwards
		public void Flee() {
			var lastInd = _fleePoints.Count - 1;
			var currentNode = _fleePoints[lastInd];
			var minimumDistance = 3f;

			if (_fleePoints.Count < 2 && _useCollider) {
				minimumDistance = 1.0f + _player.GetColliderExtents();

				var directionToPlayer = (_player.transform.position - _target).normalized;
				var bufferDistance = directionToPlayer * minimumDistance;

				currentNode = _target + bufferDistance;
			}

			var distanceToNode = (_player.transform.position - currentNode).sqrMagnitude;

			if (distanceToNode < minimumDistance) {
				_path.Insert(0, currentNode);
				_fleePoints.RemoveAt(lastInd);
			} else {
				_player.RequestMove(currentNode);
			}
		}

		private enum Trigger {
			ApprachTarget,
			ReachedTarget,
		}

		private enum State {
			Start,
			Running,
			Finish
		}
	}
}
