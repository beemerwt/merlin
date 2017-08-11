// ReSharper disable All

namespace Merlin.Profiles.Expedition {
	partial class Expedition {
		private Mob LookupTarget(FightingObjectView target) {
			for (int i = mobs.Length - 1; i >= 0; i--)
				if (mobs[i].Equals(target))
					return mobs[i];
			return null;
		}

		private bool ShouldInterrupt(FightingObjectView target) {
			var mob = LookupTarget(target);
			return mob != null && mob.ShouldInterrupt(target.GetSpellCasted());
		}

		private bool ShouldDodge(FightingObjectView target, out Profiles.Combat.Evade evade) {
			evade = Combat.Evade.Tank; // Default
			if (target == null) return false;
			var mob = LookupTarget(target);
			if (mob == null) target.CreateTextEffect("Mob not found.");
			return mob != null && mob.ShouldDodge(target.GetSpellCasted(), out evade);
		}
	}
}
