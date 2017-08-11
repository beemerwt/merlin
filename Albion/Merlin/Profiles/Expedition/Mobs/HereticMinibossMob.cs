namespace Merlin.Profiles.Expedition.Mobs {
	class MageMinibossMob : Mob {
		public override string Name => "MOB_HERETIC_MAGE_MINIBOSS_01";

		public override Spell[] Spells => new Spell[1] {
			new Spell(gz.SpellCategory.Damage, gz.SpellTarget.Ground, Combat.Evade.Behind)
		};
	}
}
