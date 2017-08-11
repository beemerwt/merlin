using SpellCategory = gz.SpellCategory;
using SpellTarget = gz.SpellTarget;
using Evade = Merlin.Profiles.Combat.Evade;
// ReSharper disable All

namespace Merlin.Profiles.Expedition {
	abstract class Mob {

		public class Spell {
			public SpellCategory Category { get; }
			public SpellTarget Target { get; }
			public Evade Evade { get; }
			public string Name { get; }

			public bool Interruptable { get; set; }

			public Spell(SpellCategory category, SpellTarget target, Evade evade, string name = null) {
				Category = category;
				Target = target;
				Evade = evade;
				Name = name;
				Interruptable = true;
			}

			public bool Equals(gz spell) {
				return Category == spell.d4 && Target == spell.d1 && string.Equals(Name, spell.d6);
			}
		}

		public abstract string Name { get; }
		public abstract Spell[] Spells { get; }

		public Spell LookupSpell(gz spell) {
			if (spell == null) return null;
			for (int i = Spells.Length - 1; i >= 0; i--) {
				if (Spells[i].Equals(spell))
					return Spells[i];
			}

			return null;
		}

		public bool Equals(FightingObjectView view) => view.PrefabName.Equals(Name);

		public bool ShouldDodge(gz spell, out Evade evade) {
			evade = Evade.Tank;
			var mobSpell = LookupSpell(spell);
			if (mobSpell == null) return false;
			evade = mobSpell.Evade;
			return true;
		}

		public bool ShouldInterrupt(gz spell) {
			var mobSpell = LookupSpell(spell);
			return mobSpell != null && mobSpell.Interruptable;
		}
	}
}
