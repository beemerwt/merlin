namespace Merlin.Profiles.Expedition.Mobs {
	class HereticArcherMob : Mob {
		public override string Name => "MOB_HERETIC_ARCHER_01";
		public override Spell[] Spells => new Spell[1] {
			new Spell(gz.SpellCategory.Damage, gz.SpellTarget.Ground, Combat.Evade.Left)
		};
	}
}
