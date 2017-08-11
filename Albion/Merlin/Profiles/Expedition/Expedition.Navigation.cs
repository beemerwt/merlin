using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin.Profiles.Expedition {
	partial class Expedition {

		ExpeditionAgentObjectView GetEntrancePortal() {
			List<ExpeditionAgentObjectView> expeditionAgentList = _client.GetEntities<ExpeditionAgentObjectView>(IsValidAgent);
			ExpeditionAgentObjectView agent = expeditionAgentList.FirstOrDefault();
			return agent;
		}

		ExpeditionExitObjectView GetExitPortal() {
			List<ExpeditionExitObjectView> portalList = _client.GetEntities<ExpeditionExitObjectView>(IsValidPortal);
			ExpeditionExitObjectView agent = portalList.FirstOrDefault();
			return agent;
		}

		bool IsValidAgent(ExpeditionAgentObjectView agent) {
			return agent != null;
		}

		bool IsValidPortal(ExpeditionExitObjectView portal) {
			return portal != null;
		}

		Vector3 GetDungeonEndPoint() {
			return T3_EXPEDITION_ENDING_POINT;
		}

		void FindPathToEnd() {
			targetPoint = GetDungeonEndPoint();
			_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), targetPoint, IsBlocked, out targetPoints);
			path = new ExpeditionPathingRequest(_localPlayerCharacterView, targetPoint, targetPoints);
			pathFound = true;
		}

		public bool IsBlocked(Vector2 location) {
			var vector = new Vector3(location.x, 0, location.y);

			if (_localPlayerCharacterView != null) {
				var playerLocation = new Vector2(_localPlayerCharacterView.transform.position.x, _localPlayerCharacterView.transform.position.z);
				var distance = (playerLocation - location).magnitude;

				if (distance < 2f)
					return false;
			}

			return (_client.Collision.GetFlag(vector, 1.0f) > 0);
		}
	}
}
