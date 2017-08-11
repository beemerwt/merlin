namespace Merlin.Profiles.Expedition.Mobs {
	class HereticTankMob : Mob {
		public override string Name => "MOB_HERETIC_TANK_01";
		public override Spell[] Spells => new Spell[1] {
			new Spell(gz.SpellCategory.CrowdControl, gz.SpellTarget.Ground, Combat.Evade.Behind, "melee_shieldslam")
		};
	}
}
