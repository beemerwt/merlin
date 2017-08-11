using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Merlin.API;
using UnityEngine;
using Player = LocalPlayerCharacterView;
using SpellCategory = gz.SpellCategory;
using SpellTarget = gz.SpellTarget;

namespace Merlin.Profiles {

	public class Combat {

		public static Combat Instance;
		private static Player Player => Client.Instance.LocalPlayerCharacter;

		static Combat() {
			Instance = new Combat();
		}

		public delegate bool InterruptDelegate(FightingObjectView target);
		public delegate bool DodgeDelegate(FightingObjectView target, out Evade evade);

		private InterruptDelegate ShouldInterrupt;
		private DodgeDelegate ShouldDodge;

		private State state;

		private float elapsedSeconds;
		private float secondCount;

		private bool evading;

		public void SetState(State newState) => state = newState;
		public bool IsState(State state) => this.state == state;
		public State GetState() => state;

		private Combat() {
			SetState(State.Idle);
			ResetDelegates();
		}

		public void Debug() {
			var target = Player.GetAttackTarget();
			var guiX = 1373;
			var guiY = 182;
			var guiW = 304;
			var guiH = 206; // 9 label lines.
			var boxGui = new Rect(guiX, guiY, guiW, guiH);
			GUI.Box(boxGui, "");

			var useSpells = GetUsableSpells();
			var spellStr = "";
			for (int i = useSpells.Length - 1; i >= 0; i--) {
				if (string.IsNullOrEmpty(useSpells[i].Name)) continue;
				spellStr += useSpells[i].Name + "\n\t";
			}
			var dbgStr = $"Target: {target.PrefabName}\nSpells: {spellStr}\n";
			GUI.Label(new Rect(guiX + 4, guiY + 4, guiW - 8, guiH - 8), dbgStr);
		}

		public void Update() {
			var curTime = Stopwatch.GetTimestamp() / Stopwatch.Frequency; // Gets seconds
			elapsedSeconds += curTime - secondCount;
			secondCount = curTime;

			// Update based on state
			// Do nothing while idle.
			switch (state) {
				case State.Idle:
					Idle();
					break;
				case State.Combat:
					Fight();
					break;
				case State.Respawn:
					Respawn();
					break;
				case State.Recover:
					Recover();
					break;
				case State.Flee:
					Flee();
					break;
			}
		}

		private void Idle() {
			if (Player.IsUnderAttack(out FightingObjectView attacker)) {
				Core.Log("[Combat] Attacked");
				elapsedSeconds = 0;
				SetState(State.Combat);
				return;
			}

			if (Player.GetHealth() <= 0) {
				Core.Log("[Combat] Player died");
				SetState(State.Respawn);
				return;
			}

			if (Player.GetHealth() <= Player.GetMaxHealth() * 0.5f) {
				Core.Log("[Combat] Recovering");
				SetState(State.Recover);
				return;
			}
		}

		private void Fight() {
			var target = Player.GetAttackTarget();
			if (ShouldDodge(target, out Evade evade)) {
				Dodge(target, evade, ShouldInterrupt(target));
				return;
			}

			if (!evading)
				Attack(target);
		}

		private void Attack(FightingObjectView target) {
			var attackTimer = Player.GetAttackDelay().p();
			var spells = GetUsableSpells();
			Player.CreateTextEffect("[Attacking]");

			// enemy not null and player has finished autoAttacking
			if (target != null && elapsedSeconds > (attackTimer / 1000f) * 2) {
				var selfBuffSpells = spells.Target(SpellTarget.Self).Category(SpellCategory.Buff);
				if (selfBuffSpells.Any() && !Player.IsCastingSpell()) {
					Player.CreateTextEffect("[Casting Buff Spell]");
					Player.CastOnSelf(selfBuffSpells.FirstOrDefault().SpellSlot);
					elapsedSeconds = 0;
					return;
				}

				var selfDamage = spells.Target(SpellTarget.Self).Category(SpellCategory.Damage);
				if (selfDamage.Any() && !Player.IsCastingSpell()) {
					Player.CreateTextEffect("[Casting Buff Spell]");
					Player.CastOnSelf(selfDamage.FirstOrDefault().SpellSlot);
					elapsedSeconds = 0;
					return;
				}

				var enemyBuffSpells = spells.Target(SpellTarget.Enemy).Category(SpellCategory.Buff);
				if (enemyBuffSpells.Any() && !Player.IsCastingSpell()) {
					Player.CreateTextEffect("[Casting Damage Spell]");
					Player.CastOn(enemyBuffSpells.FirstOrDefault().SpellSlot, target);
					elapsedSeconds = 0;
					return;
				}

				var enemyCCSpells = spells.Target(SpellTarget.Enemy).Category(SpellCategory.CrowdControl);
				if (enemyCCSpells.Any() && !Player.IsCastingSpell()) {
					Player.CreateTextEffect("[Casting CrowdControl Spell]");
					Player.CastOn(enemyCCSpells.FirstOrDefault().SpellSlot, target);
					elapsedSeconds = 0;
					return;
				}

				/* No Longer Working
				var groundDamageSpells = spells.Target(gy.SpellTarget.Ground).Category(gy.SpellCategory.Damage);
				if (groundDamageSpells.Any()) {
					player.CreateTextEffect("[Casting Ground Spell]");
					player.CastAt(groundDamageSpells.FirstOrDefault().SpellSlot, attackTarget.transform.position);
					return;
				} */
			}

			if (Player.IsUnderAttack(out FightingObjectView attacker)) {
				Player.SetSelectedObject(attacker);
				Player.AttackSelectedObject();
				return;
			}

			if (Player.GetHealth() <= (Player.GetMaxHealth() * 0.1f)) {
				SetState(State.Flee);
				return;
			}

			if (Player.IsCasting())
				return;

			Core.Log("Continuing.");
			SetState(State.Idle);
		}

		private void Dodge(FightingObjectView target, Evade evade, bool shouldInterrupt = true) {
			Player.CreateTextEffect("[Dodging]");
			var spells = GetUsableSpells();
			var defensive = spells.FirstOrDefault(s => s.Category.Equals(SpellCategory.Buff_Damageshield));
			var interrupt = spells.FirstOrDefault(s => s.Category.Equals(SpellCategory.CrowdControl) && !s.Target.Equals(SpellTarget.Ground));

			if (interrupt != null && shouldInterrupt) {
				Cast(interrupt);
				Core.Log("Evading by Interrupt");
				return;
			}
			Vector3 movePos;
			switch (evade) {
				case Evade.Behind:
					movePos = target.transform.position - target.transform.forward * 20f;
					Player.RequestMove(movePos);
					evading = true;
					Core.Log("Evading behind");
					break;
				case Evade.Left:
					movePos = target.transform.position - target.transform.right * 20f;
					Player.RequestMove(movePos);
					evading = true;
					Core.Log("Evading left");
					break;
				case Evade.Away:
					movePos = target.transform.position - target.transform.forward * 20f;
					Player.RequestMove(movePos);
					evading = true;
					Core.Log("Evading away");
					break;
				case Evade.Defensive:
					Cast(defensive);
					Core.Log("Evading by defensive");
					break;
			}
		}

		private void Recover() {
			var recoverySpell = Player.GetSpells().Slot(SpellSlotIndex.Armor).FirstOrDefault();

			if (Player.IsUnderAttack(out FightingObjectView attacker)) {
				Core.Log("Attacked");
				SetState(State.Combat);
				return;
			}

			if (recoverySpell != null && !Player.IsGettingUpFromKnockDown() &&
			    recoverySpell.Name.Equals("OUTOFCOMBATHEAL") && recoverySpell.IsReady) {
				Player.CastOnSelf(SpellSlotIndex.Armor);
			}

			if (Player.GetHealth() <= 0)
				SetState(State.Respawn);

			if (Player.GetHealth() > Player.GetMaxHealth() * 0.75f) {
				SetState(State.Idle);
			}
		}

		/**
		 * Flee is a virtual, but protected, method so it *can* be overridden.
		 * This can be used to implement your own methods in separate classes.
		 */
		protected virtual void Flee() {
			if (Player.GetHealth() <= 0)
				SetState(State.Respawn);
			if (Player.GetHealth() == Player.GetMaxHealth())
				SetState(State.Idle);

		}

		private void Respawn() {
			var isRespawnShowing = GameGui.Instance.RespawnGui.ExpeditionStart.isActiveAndEnabled;
			if (isRespawnShowing) {
				Player.OnRespawn();
				SetState(State.Idle);
			}
		}

		private Spell[] GetUsableSpells() {
			List<Spell> returnSpells = new List<Spell>();
			var spells = Player.GetSpells();
			for (int i = spells.Length - 1; i > 0; i--) {
				if (!spells[i].IsReady) continue;
				if (spells[i].Target == SpellTarget.Ground) continue; // Not quite working.
				if (spells[i].SpellSlot == SpellSlotIndex.Potion || spells[i].SpellSlot == SpellSlotIndex.Food)
					continue;
				returnSpells.Add(spells[i]);
			}

			return returnSpells.ToArray();
		}

		private void Cast(Spell spell, FightingObjectView target = null) {
			switch (spell.Target) {
				case SpellTarget.Self:
					Player.CastOnSelf(spell.SpellSlot);
					return;
				case SpellTarget.Ground:
					Player.CastAt(spell.SpellSlot, target == null ? Player.GetPosition() : target.GetPosition());
					return;
				case SpellTarget.Enemy:
					if (target != null)
						Player.CastOn(spell.SpellSlot, target);
					return;
			}
		}

		public void ResetDelegates() {
			ResetInterruptDelegate();
			ResetDodgeDelegate();
		}

		public void SetInterruptDelegate(InterruptDelegate shouldInterrupt) => ShouldInterrupt = shouldInterrupt;
		public void SetDodgeDelegate(DodgeDelegate dodgeDelegate) => ShouldDodge = dodgeDelegate;
		public void ResetInterruptDelegate() => ShouldInterrupt = DefaultShouldInterrupt;
		public void ResetDodgeDelegate() => ShouldDodge = DefaultShouldDodge;

		private bool DefaultShouldDodge(FightingObjectView target, out Evade evade) {
			evade = Evade.Tank;
			if (target.IsCasting()) return false;

			var spellCategory = target.GetSpellCasted().d4;
			var spellTarget = target.GetSpellCasted().d1;

			if (spellCategory != SpellCategory.Damage) return true;

			if (spellTarget == SpellTarget.Enemy)
				evade = Evade.Defensive;
			if (spellTarget == SpellTarget.Ground)
				evade = Evade.Left;

			return true;
		}

		private bool DefaultShouldInterrupt(FightingObjectView target) {
			return true;
		}

		public enum State {
			Idle,
			Combat,
			Flee,
			Respawn,
			Recover,
			Debug
		}

		public enum Evade {
			Behind,
			Left,
			Defensive,
			Tank,
			Away
		}

		public static string GetStateString(State state) {
			switch (state) {
				case State.Idle:
					return "Idle";
				case State.Combat:
					return "Combat";
				case State.Flee:
					return "Flee";
				case State.Respawn:
					return "Respawn";
				case State.Recover:
					return "Recover";
				case State.Debug:
					return "Debug";
				default:
					return "Unknown";
			}
		}
	}
}
