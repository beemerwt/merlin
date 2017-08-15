using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

/**
 * Not Yet working in almost any regard.
 * The only remote thing that is working is the label, so far...
 * and it's font isn't even correct. Only the color and size.
 */

namespace Merlin.Merlingue {
	class AlbionStyle {
		public static GUISkin Skin { get; }

		static AlbionStyle() {
			Skin = new GUISkin {
				font = GameGui.Instance.HeroStatsGui.NameLabel.trueTypeFont,
				label = {
					fontStyle = GameGui.Instance.HeroStatsGui.NameLabel.fontStyle,
					fontSize = GameGui.Instance.HeroStatsGui.NameLabel.fontSize
				},
				button = { fontStyle = GameGui.Instance.HeroStatsGui.NameLabel.fontStyle }
			};
		}
	}
}
