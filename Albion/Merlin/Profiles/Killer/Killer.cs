using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Merlin.MerlinGui;
using UnityEngine;
using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin.Profiles.Killer {
	partial class Killer : Profile {
		private static Combat Combat => Combat.Instance;
		public override string Name => "Killer";
		private LocalPlayerCharacterView player => _localPlayerCharacterView;
		private static State state;

		private const float GUI_X = 70;
		private const float GUI_Y = 135;
		private const float GUI_W = 296;

		public static void SetState(State newState) => state = newState;
		public static State GetState() => state;

		private MobView curTarget;
		private ClusterPathingRequest mobPath;
		private int mobKillCount = 0;

		private Menu citySelect;
		private Rect posRect, killsRect, stateRect, combatStateRect, mobNameRect, spellStringRect;
		private Rect newBox, newStartRect, newStopRect;

		private float standingTime;
		private float secondCount;
		private Vector3 oldPosition;

		protected override void OnStart() {
			var addY = 4;
			posRect = new Rect(GUI_X + 4, GUI_Y + addY, GUI_W, 20);
			killsRect = new Rect(GUI_X + 4, GUI_Y + (addY += 22), GUI_W, 20);
			stateRect = new Rect(GUI_X + 4, GUI_Y + (addY += 22), GUI_W, 20);
			combatStateRect = new Rect(GUI_X + 4, GUI_Y + (addY += 22), GUI_W, 20);
			mobNameRect = new Rect(GUI_X + 4, GUI_Y + (addY += 22), GUI_W, 20);
			spellStringRect = new Rect(GUI_X + 4, GUI_Y + (addY += 22), GUI_W, 20);

			citySelect = new Menu(new Rect(GUI_X + 4, GUI_Y + (addY += 22), GUI_W - 4, 30), "Repair City", "Fort Sterling", "Bridgewatch");
			Core.Log("Starting Killer");

			var full = citySelect.GetFullHeight();
			newStartRect = new Rect(GUI_X + 4, GUI_Y + addY + full, GUI_W / 2 - 4, 30);
			newStopRect = new Rect(GUI_X + (GUI_W / 2) + 4, GUI_Y + addY + full, GUI_W / 2 - 4, 30);
			newBox = new Rect(GUI_X, GUI_Y, GUI_W + 4, addY + 34 + full);
			ProfileSelector.SetBoxRect(newBox);
			ProfileSelector.SetStartButtonRect(newStartRect);
			ProfileSelector.SetStopButtonRect(newStopRect);

			newBox = new Rect(GUI_X, GUI_Y, GUI_W + 4, addY + 34 + full);
		}

		protected override void OnStop() {

		}

		protected override void OnUpdate() {

			Combat.Update();

			if (!Combat.IsState(Combat.State.Idle)) return;

			// Do updates.
			switch (state) {
				case State.Searching:
					Search();
					break;
				case State.Moving:
					Move();
					break;
				case State.Repair:
					Repair();
					break;
			}
		}

		private void Search() {
			if (GetNextTarget(out MobView mob))
				curTarget = mob;

			if (mob != null && ValidateMob(curTarget)) {
				Core.Log("Moving");
				SetState(State.Moving);
			}
		}

		private void Move() {
			// Mob is dead if we are "stuck" in Move state...
			if (oldPosition == player.GetPosition()) {
				var curTime = Stopwatch.GetTimestamp() / Stopwatch.Frequency; // Gets seconds
				standingTime += curTime - secondCount;
				secondCount = curTime;

				if (standingTime > 20) {
					standingTime = 0;
					SetState(State.Searching);
					curTarget = null;
					return;
				}
			} else
				standingTime = 0;

			if (!ValidateMob(curTarget)) {
				Core.Log("Searching");
				curTarget = null;
				SetState(State.Searching);
				return;
			}

			if (mobPath != null) {
				if (mobPath.IsRunning)
					mobPath.Continue();
				else
					mobPath = null;
				return;
			}

			/* Begin moving closer the target. */
			var playerCenter = player.transform.position;
			var targetCenter = curTarget.transform.position;

			var distance = (targetCenter - playerCenter).magnitude;
			var minimumDistance = curTarget.GetColliderExtents() + player.GetColliderExtents() + 1.5f;

			if (distance >= minimumDistance) {
				if (player.TryFindPath(new ClusterPathfinder(), curTarget, IsBlocked, out List<Vector3> pathing))
					mobPath = new ClusterPathingRequest(player, curTarget, pathing);
				else {
					SetState(State.Searching);
				}

				return;
			}

			if (player.IsInCombat())
				return;

			player.CreateTextEffect("[Attacking]");
			player.SetSelectedObject(curTarget);
			player.AttackSelectedObject();
		}

		public bool IsBlocked(Vector2 location) {
			var vector = new Vector3(location.x, 0, location.y);

			if (curTarget != null) {
				var resourcePosition = new Vector2(curTarget.transform.position.x,
					curTarget.transform.position.z);
				var distance = (resourcePosition - location).magnitude;

				if (distance < (curTarget.GetColliderExtents() + player.GetColliderExtents()))
					return false;
			}

			if (player != null) {
				var playerLocation = new Vector2(player.transform.position.x, player.transform.position.z);
				var distance = (playerLocation - location).magnitude;

				if (distance < 2f)
					return false;
			}

			return (_client.Collision.GetFlag(vector, 1.0f) > 0);
		}

		private bool GetNextTarget(out MobView mob) {
			List<MobView> mobs = _client.GetEntities<MobView>(ValidateMob);
			mob = mobs.Where(m => m.Mob.sd().d2() == er.MobAlignments.Hostile).OrderBy((view) => {
				var playerPosition = player.transform.position;
				var mobPosition = view.transform.position;

				var score = (mobPosition - playerPosition).sqrMagnitude;
				var yDelta = Math.Abs(_landscape.GetLandscapeHeight(playerPosition.c()) - _landscape.GetLandscapeHeight(mobPosition.c()));
				score += (yDelta * 10f);

				return (int)score;
			}).FirstOrDefault();

			return mob != null;
		}

		private bool ValidateMob(MobView mob) {
			if (mob.IsDead())
				return false;
			return true;
		}

		private void OnGUI() {
			var playerPos = player.GetPosition();
			GUI.Label(posRect, "Position: " + playerPos.x + ", " + playerPos.y + ", " + playerPos.z);
			GUI.Label(killsRect, "Kills: " + mobKillCount);
			GUI.Label(stateRect, "State: " + GetStateString(state));
			GUI.Label(combatStateRect, "Combat State: " + Combat.GetStateString(Combat.GetState()));
			GUI.Label(mobNameRect, "MOB_NAME: " + Combat.GetCurrentMobName());
			GUI.Label(spellStringRect, Combat.GetSpellString());

			citySelect.Draw();
		}

		private string GetStateString(State state) {
			switch (state) {
				case State.Searching:
					return "Searching";
				case State.Moving:
					return "Moving";
				case State.Repair:
					return "Repairing";
			}

			return "";
		}

		internal enum State {
			Searching,
			Moving,
			Repair
		}
	}
}
