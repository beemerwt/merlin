namespace Merlin.Profiles.Expedition.Mobs {
	class OverseerMob : Mob {
		public override string Name => "MOB_HERETIC_OVERSEER_BOSS_01";
		public sealed override Spell[] Spells => new Spell[2] {
			new Spell(gz.SpellCategory.Damage, gz.SpellTarget.Ground, Combat.Evade.Away),
			new Spell(gz.SpellCategory.Damage, gz.SpellTarget.Ground, Combat.Evade.Behind, "melee_standardstrike")
		};

		public OverseerMob() {
			Spells[1].Interruptable = false;
		}
	}
}
