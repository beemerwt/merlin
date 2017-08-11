using System.Collections.Generic;
using System.Linq;
using Merlin.API;

namespace Merlin.Profiles.Expedition {
	partial class Expedition {

		private bool pathFound;

		void Restart() {
			if (NeedRepair())
				SetState(State.Repair);

			if (!IsInExpedition())
				EnterDungeon();
			else {
				if (_client.State != GameState.Playing) return;
				if (!pathFound && elapsedSeconds > 5) {
					// 5 sec to load exped
					FindPathToEnd();
					SetState(State.Continue);
				}
			}
		}

		bool IsInExpedition() {
			if (_world.CurrentCluster == null)
				return true;
			return false;
		}

		void EnterDungeon() {
			var player = _localPlayerCharacterView;
			Core.Log("[Expedition] Entering Dungeon.");

			var agentDialogOpened = GameGui.Instance.ExpeditionAgentGui.isActiveAndEnabled;
			var entryDialogOpened = GameGui.Instance.ExpeditionAgentGui.ExpeditionAgentDetailsGui.isActiveAndEnabled;

			var agent = GetEntrancePortal();

			// Walk to agent.
			if (!agentDialogOpened) {
				player.Interact(agent);
				elapsedSeconds = 0;
			}

			if (agentDialogOpened && !entryDialogOpened && elapsedSeconds > 0.2f) {
				var entries = GameGui.Instance.ExpeditionAgentGui.ExpeditionSelectionList;
				entries[0].OnClicked();
				elapsedSeconds = 0;
			}

			if (agentDialogOpened && entryDialogOpened && elapsedSeconds > 0.2f) {
				GameGui.Instance.ExpeditionAgentGui.ExpeditionAgentDetailsGui?.OnRegistrationClicked();
				elapsedSeconds = 0;
			}
		}

		bool NeedRepair() {
			Core.Log("[Expedition] Checking if player needs to repair.");
			return false;
		}

		RepairBuildingView GetRepairBuilding() {
			List<RepairBuildingView> repairBuildingList = _client.GetEntities<RepairBuildingView>(IsValidRepair);
			RepairBuildingView repairBuilding = repairBuildingList.FirstOrDefault();
			return repairBuilding;
		}

		bool IsValidRepair(RepairBuildingView view) {
			return view != null;
		}

		private bool hasRepaired = false;

		void Repair() {
			var player = _localPlayerCharacterView;

			var repairDialogOpened = GameGui.Instance.BuildingUsageAndManagementGui.BuildingUsage.RepairItemView.isActiveAndEnabled;
			var payDialogOpened = GameGui.Instance.PaySilverDetailGui.isActiveAndEnabled;

			RepairBuildingView rbv = GetRepairBuilding();
			var minimumDistance = rbv.GetColliderExtents() + player.GetColliderExtents();

			var directionToPlayer = (player.transform.position - rbv.transform.position).normalized;
			var bufferDistance = directionToPlayer * minimumDistance;

			var currentNode = rbv.transform.position + bufferDistance;

			if (!repairDialogOpened) {
				Core.Log("Moving to RBV");
				player.Interact(rbv);
			}

			if (repairDialogOpened && !hasRepaired) {
				Core.Log("Using RBV");
				GameGui.Instance.BuildingUsageAndManagementGui.BuildingUsage.RepairItemView.OnClickRepairAllButton();
			}

			if (payDialogOpened && !hasRepaired) {
				Core.Log("Paying...");
				GameGui.Instance.PaySilverDetailGui.OnPay();
				hasRepaired = true;
				elapsedSeconds = 0;
			}

			if (hasRepaired && elapsedSeconds > 5) {
				Core.Log("Finished Repairing");
				hasRepaired = false;
				SetState(State.Restart);
			}
		}
	}
}
