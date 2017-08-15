using System;
using UnityEngine;

namespace Merlin.Merlingue {
	class Button : GuiElement, IClickable {
		private Action clickAction;

		public Button(Rect dimensions, string label, Action action) : base(dimensions, label) {
			clickAction = action;
		}

		public override bool Draw() => GUI.Button(dimensions, label);
		public override bool Draw(GUIStyle style) => GUI.Button(dimensions, label, style);

		void IClickable.OnClicked() {
			clickAction();
		}
	}
}
