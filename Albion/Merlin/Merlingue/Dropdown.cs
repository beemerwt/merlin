using System.Collections.Generic;
using UnityEngine;

namespace Merlin.Merlingue {
	class Dropdown : GuiElement, IClickable {

		public TextAnchor Alignment { get; set; }

		protected List<Button> dropdownButtons;
		protected bool isDropped;

		public Dropdown(Rect dimensions, string label, params Button[] entries) : base(dimensions, label) {
			dropdownButtons = new List<Button>(entries);
			Alignment = TextAnchor.MiddleLeft; // Default alignment
		}

		public Dropdown(Vector2 pos, string label, params Button[] entries)
			: this(new Rect(pos.x, pos.y, DEFAULT_W, DEFAULT_H), label, entries) { }

		protected virtual void DrawButtons() {
			var prevAlignment = GUI.skin.label.alignment;
			GUI.skin.button.alignment = Alignment;
			if (!isDropped) return;
			for (var i = 0; i < dropdownButtons.Count; i++)
				if (dropdownButtons[i].Draw())
					(dropdownButtons[i] as IClickable).OnClicked();
			GUI.skin.button.alignment = prevAlignment;
		}

		protected void DrawArrow() {
			var prevAlignment = GUI.skin.label.alignment;
			GUI.skin.label.alignment = TextAnchor.MiddleRight;
			GUI.Label(dimensions, "▼  ");
			GUI.skin.label.alignment = prevAlignment;
		}

		protected virtual void DrawMenuButton() {
			var prevAlignment = GUI.skin.button.alignment;
			GUI.skin.button.alignment = Alignment;
			if (GUI.Button(dimensions, label))
				(this as IClickable).OnClicked();
			GUI.skin.button.alignment = prevAlignment;

			DrawArrow();
		}

		public override bool Draw() {
			DrawMenuButton();
			DrawButtons();
			return true;
		}

		public override bool Draw(GUIStyle style) => Draw();

		/// <summary>
		/// Adds <paramref name="button"/> to the list.
		/// </summary>
		/// <param name="button">New button option for the dropdown</param>
		public void AddItem(Button button) => dropdownButtons.Add(button);
		
		/// <summary>
		/// Gets the height of the entire dropdown, including the buttons when it is dropped down.
		/// </summary>
		public float GetFullHeight() {
			var height = dimensions.height;
			for (var i = dropdownButtons.Count - 1; i > 0; i++)
				height += dropdownButtons[i].GetDimensions().height;
			return height;
		}

		/// <summary>
		/// Returns the y coordinate directly underneath the next button.
		/// </summary>
		public virtual Vector2 GetNextPosition() {
			var returnX = dimensions.x;
			var returnY = dimensions.y + dimensions.height + GetFullHeight();
			return new Vector2(returnX, returnY);
		}

		public void OnClicked() => isDropped = !isDropped;
	}
}
