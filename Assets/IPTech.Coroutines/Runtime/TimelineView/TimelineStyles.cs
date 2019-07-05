/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using UnityEngine;

namespace IPTech.Coroutines.Insights {
	public class TimelineStyle {
		GUISkin _skin;

		public TimelineStyle(GUISkin skin) {
			_skin = skin;
		}

		public Texture2D ResizeUpDown {
			get {
				if(_skin!=null) {
					return _skin.GetStyle("ResizeUpDown").normal.background;
				}
				return null;
			}
		}

		public GUISkin Skin {
			get {
				return _skin;
			}
		}

		public GUIStyle Row {
			get {
				if(_skin!=null) {
					return _skin.GetStyle("Row");
				}
				return GUIStyle.none;
			}
		}

		public GUIStyle Dark {
			get { 
				if(_skin!=null) {
					return _skin.GetStyle("Dark");
				}
				return GUIStyle.none;
			}
		}

		public GUIStyle EntryText {
			get {
				if(_skin!=null) {
					return _skin.GetStyle("EntryText");
				}
				return GUI.skin.label;
			}
		}

		public GUIStyle GetEntry(TimelineEntryState state) {
			if(_skin == null) return GUI.skin.button;

			switch(state) {
				case TimelineEntryState.Attention:
					return _skin.GetStyle("EntryAttention");
				case TimelineEntryState.Highlight:
					return _skin.GetStyle("EntryHighlight");
				default:
					return _skin.GetStyle("Entry");
			}
		}

		public GUIStyle EntryToggle {
			get {
				if(_skin!=null) {
					return _skin.toggle;
				}
				return GUI.skin.toggle;
			}
		}

		public GUIStyle Footer {
			get {
				if(_skin!=null) {
					return _skin.box;
				}
				return GUI.skin.box;
			}
		}

		public GUIStyle GetChildEntry(TimelineEntryState state) {
			if(_skin == null) return GUI.skin.button;

			switch(state) {
				case TimelineEntryState.Attention:
					return _skin.GetStyle("ChildEntryAttention");
				case TimelineEntryState.Highlight:
					return _skin.GetStyle("ChildEntryHighlight");
				default:
					return _skin.GetStyle("ChildEntry");
			}
		}

		public GUIStyle Background {
			get {
				if(_skin!=null) {
					return _skin.GetStyle("Background");
				}
				return GUIStyle.none;
			}
		}

		public GUIStyle Tickmarks {
			get {
				if(_skin!=null) {
					return _skin.GetStyle("Tickmarks");
				}
				return GUIStyle.none;
			}
		}

		/*
		Texture2D GenerateBoxTexture(int widthPixels, int heighPixels, Color borderColor, Color backgroundColor, int borderWidth) {
			Texture2D tex = new Texture2D(widthPixels, heighPixels);
			Color[] cols = tex.GetPixels();
			for(int i = 0; i < cols.Length; i++) {
				int row = i / widthPixels;
				int col = i % widthPixels;
				if(
					(row < borderWidth || row > heighPixels - borderWidth - 1) ||
					(col < borderWidth || col > widthPixels - borderWidth - 1)
				) {
					cols[i] = borderColor;
				} else {
					cols[i] = backgroundColor;
				}
			}
			tex.SetPixels(cols);
			tex.Apply();
			return tex;
		}*/
	}
}
