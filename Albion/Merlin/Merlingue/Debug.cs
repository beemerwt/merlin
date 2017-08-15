using UnityEngine;

namespace Merlin.Merlingue {
	class Debug : MonoBehaviour {

		private const float GUI_X = 70;
		private const float GUI_Y = 135;
		private const float GUI_W = 296;

		private Rect albionLabelRect;
		private Rect albionButtonRect;

		private Button dbgButton;
		private Dropdown dbgDropdown;
		private SelectionDropdown dbgSelectionDropdown;

		public void Start() {
			var elementGuiX = GUI_X + 4;
			var elementGuiY = GUI_Y + 4;
			var elementGuiW = GUI_W - 8;
			
			// Instantiate rects
			albionLabelRect = new Rect(elementGuiX, elementGuiY, GUI_W - 8, 18);
			albionButtonRect = new Rect(elementGuiX, elementGuiY+=22, GUI_W - 8, 30);

			var dbgButtonRect = new Rect(elementGuiX, elementGuiY+=34, GUI_W - 8, 30);
			var dbgDropdownRect = new Rect(elementGuiX, elementGuiY+=34, GUI_W - 8, 30);
			var dbgSelDropdownRect = new Rect(elementGuiX, elementGuiY+=34, GUI_W - 8, 30);

			// Instantiate items...
			dbgButton = new Button(dbgButtonRect, "Test button", () => Core.Log("This test button worked."));
			dbgDropdown = new Dropdown(dbgDropdownRect, "Test button");

			// Create buttons for the "Dropdown"
			var commonSize = new Vector3(elementGuiW, 30);

			var dropB1Rect = new Rect(dbgDropdown.GetNextPosition(), commonSize);
			var dbgDropdownButton1 = new Button(dropB1Rect, "DropdownButton1", () => Core.Log("DropdownButton1 Worked."));
			dbgDropdown.AddItem(dbgDropdownButton1);

			var dropB2Rect = new Rect(dbgDropdown.GetNextPosition(), commonSize);
			var dbgDropdownButton2 = new Button(dropB2Rect, "DropdownButton2", () => Core.Log("DropdownButton2 Worked."));
			dbgDropdown.AddItem(dbgDropdownButton2);

			// Instantiate a SelectionDropdown
			dbgSelectionDropdown = new SelectionDropdown(dbgSelDropdownRect, "Test button", "Fort Sterling", "Bridgewatch");
		}

		public void OnGUI() {
			// Start by testing the elements inside Merlingue
			dbgButton.Draw();
			dbgDropdown.Draw();
			dbgSelectionDropdown.Draw();

			// Then test the AlbionStyle elements.
			// Save them
			var oldLabelStyle = GUI.skin.label;
			var oldButtonStyle = GUI.skin.button;

			// Use the new ones
			GUI.skin.label = AlbionStyle.Skin.label;
			GUI.skin.button = AlbionStyle.Skin.button;

			// Draw them
			GUI.Label(albionLabelRect, "This is a test AlbionStyle Label.");
			GUI.Button(albionButtonRect, "This is a test AlbionStyle Button.");

			// Reset.
			GUI.skin.label = oldLabelStyle;
			GUI.skin.button = oldButtonStyle;
		}
	}
}
