using System.Collections.Generic;
using UnityEngine;

namespace Merlin.Merlingue {
	class SelectionDropdown : Dropdown {
		
		public SelectionDropdown(Rect dimensions, string label, params string[] entries) : base(dimensions, label) {
			this.label = label;
			dropdownButtons = new List<Button>();
			InstantiateButtons(entries);
		}

		private void Select(string entry) {
			label = entry;
			isDropped = false;
		}

		private void InstantiateButtons(params string[] entries) {
			foreach (var entry in entries)
				AddItem(entry);
		}

		/// <summary>
		/// Creates a new item for the dropdown.
		/// </summary>
		/// <param name="label">Item label</param>
		public void AddItem(string label) {
			var size = new Vector2(dimensions.width, dimensions.height);
			var position = GetNextPosition();
			var itemRect = new Rect(position, size);
			dropdownButtons.Add(new Button(itemRect, label, () => Select(label)));
		}

		/// <summary>
		/// Returns currently selected item's label
		/// </summary>
		public string GetSelection() => label;
	}
}
