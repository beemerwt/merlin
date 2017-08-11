using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Merlin.Profiles.Expedition.Mobs;
using UnityEngine;

namespace Merlin.Profiles.Expedition {
	public partial class Expedition : Profile {

		public enum State {
			Continue,
			Repair,
			Restart
		}

		public override string Name => "Expedition";
		private readonly Vector3 T3_EXPEDITION_ENDING_POINT = new Vector3(-182.0243f, 0f, 115.7252f);
		private const float GUI_X = 70;
		private const float GUI_Y = 135;
		private const float GUI_W = 296;

		private Mob[] mobs = {
			new OverseerMob(),
			new MageMinibossMob(),
			new HereticTankMob(),
			new HereticMageMob(),
			new HereticArcherMob()
		};

		private static State state = State.Restart;
		public static void SetState(State newState) => state = newState;
		public static State GetState() => state;

		private static Combat Combat => Combat.Instance;

		private List<Vector3> targetPoints;
		private ExpeditionPathingRequest path;
		private Vector3 targetPoint;
		private Vector3 stillPos;

		private bool guiEnabled = true;

		private float stillTimer;
		private float lastStill;

		private float secondCount;
		private float elapsedSeconds;

		protected override void OnStart() {
			targetPoints = new List<Vector3>();
			SetState(State.Restart);
			Combat.SetDodgeDelegate(ShouldDodge);
			Combat.SetInterruptDelegate(ShouldInterrupt);
		}

		protected override void OnStop() {
			path = null;
			targetPoints = null;
			Combat.ResetDelegates();
		}

		protected override void OnUpdate() {
			var curSeconds = Stopwatch.GetTimestamp() / Stopwatch.Frequency; // Gets seconds
			elapsedSeconds += curSeconds - secondCount;
			secondCount = curSeconds;

			if (Input.GetKeyDown(KeyCode.F2))
				guiEnabled = !guiEnabled;

			// Will handle fighting.
			Combat.Update();

			if (!Combat.IsState(Combat.State.Idle)) return;

			switch (state) {
				case State.Continue:
					Continue();
					break;
				case State.Repair:
					Repair();
					break;
				case State.Restart:
					Restart();
					break;
			}
		}

		void Continue() {
			var player = _localPlayerCharacterView;

			if (!IsInExpedition())
				SetState(State.Restart);

			// Standstill timer
			var pos = player.GetPosition();
			var curSeconds = Stopwatch.GetTimestamp() / Stopwatch.Frequency; // Gets seconds
			if (pos == stillPos) {
				stillTimer += curSeconds - lastStill;
				if (stillTimer > 30) // We have stood still for 30 seconds.
					path = null;
			} else {
				stillTimer = 0;
			}

			if (path != null) {
				if (path.IsRunning) {
					path.Continue();
				} else {
					path = null;
					player.Interact(GetExitPortal());
				}
			}

			// Always update the timer, so we don't get an infinity-second "boost" accidentally
			lastStill = curSeconds;
			stillPos = player.GetPosition();
		}

		void OnGUI() {
			if (guiEnabled) {
				var playerPos = _localPlayerCharacterView.GetPosition();
				GUI.Label(new Rect(GUI_X + 8, GUI_Y, GUI_W, 18), "Position: " + playerPos.x + ", " + playerPos.y + ", " + playerPos.z);
				GUI.Label(new Rect(GUI_X + 8, GUI_Y + 18, GUI_W, 18), "Target: " + targetPoint.x + ", " + targetPoint.y + ", " + targetPoint.z);
				GUI.Label(new Rect(GUI_X + 8, GUI_Y + 36, GUI_W, 18), "State: " + Combat.GetStateString(Combat.GetState()));
				GUI.Label(new Rect(GUI_X + 8, GUI_Y + 54, GUI_W, 18), "Time: " + Convert.ToString(elapsedSeconds, CultureInfo.InvariantCulture));

				Combat.Debug();
			}
		}
	}
}
