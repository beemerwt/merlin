using System.Collections.Generic;
using Merlin.Pathing.Worldmap;
using UnityEngine;
using WorldMap;

namespace Merlin.Profiles.Killer {
	partial class Killer {

		private WorldPathingRequest _worldPathingRequest;

		private void Repair() {
			if (!player.IsMounted) {
				if (player.IsMounting())
					return;

				player.MountOrDismount();
				return;
			}

			if (_worldPathingRequest != null) {
				if (_worldPathingRequest.IsRunning) {
					if (!HandleMounting(Vector3.zero))
						return;

					_worldPathingRequest.Continue();
				} else {
					_worldPathingRequest = null;
				}

				return;
			}

			var currentCluster = _world.CurrentCluster;
			var townCluster = _world.GetCluster("Fort Sterling");

			var path = new List<WorldmapCluster>();
			var pivotPoints = new List<WorldmapCluster>();

			var worldPathing = new WorldmapPathfinder();

			if (worldPathing.TryFindPath(currentCluster, townCluster, (cluster) => false, out path, out pivotPoints, true, false))
				_worldPathingRequest = new WorldPathingRequest(currentCluster, townCluster, path);
		}

		public bool HandleMounting(Vector3 target) {
			if (!_localPlayerCharacterView.IsMounted) {
				if (_localPlayerCharacterView.IsMounting())
					return false;

				if (_localPlayerCharacterView.GetMount(out MountObjectView mount)) {
					if (target != Vector3.zero && mount.InRange(target))
						return true;

					if (mount.IsInUseRange(_localPlayerCharacterView.LocalPlayerCharacter))
						_localPlayerCharacterView.Interact(mount);
					else
						_localPlayerCharacterView.MountOrDismount();
				} else {
					_localPlayerCharacterView.MountOrDismount();
				}

				return false;
			}

			return true;
		}
	}
}
