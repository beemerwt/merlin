using UnityEngine;

namespace Merlin.Merlingue {
	abstract class GuiElement {
		public const float DEFAULT_W = 100;
		public const float DEFAULT_H = 30;

		protected Rect dimensions;
		protected string label;

		protected GuiElement(Rect dimensions, string label) {
			this.dimensions = dimensions;
			this.label = label;
		}

		public abstract bool Draw();
		public abstract bool Draw(GUIStyle style);

		public Rect GetDimensions() => dimensions;
		public string GetLabel() => label;
	}
}
