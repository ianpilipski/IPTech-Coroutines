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

	public class CoroutineDebugger : MonoBehaviour {
		TimelineView _timelineView;
		TimelineData _timelineData;
		bool _clearData;
		
		public float Margin = 5.0f;
		public GUISkin TimelineSkin;

		void Awake() {
			Initialize();
		}
		
		void Initialize() {
			_timelineData = new TimelineData();
			_timelineView = new TimelineView(TimelineSkin);
			CFunc.CoroutineCreatedListeners += CoroutineCreated;

			DontDestroyOnLoad(gameObject);
		}

		void Update() {
			if(_clearData) {
				_clearData = false;
				_timelineData.Clear();
			}
			_timelineData.Update();
		}
		
		void OnDestroy() {
			CFunc.CoroutineCreatedListeners -= CoroutineCreated;
		}

		void CoroutineCreated(ICFunc coroutine) {
			_timelineData.Add(coroutine);
		}

		void OnGUI() {
			Rect windowRect = new Rect(Margin, Margin, Screen.width - Margin * 2, Screen.height - Margin * 2);
			
			GUILayout.Window(1, windowRect, DrawWindow, "Coroutine Timeline");
		}
		
		void DrawWindow(int id) {
			using(new GUILayout.HorizontalScope()) {
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Clear", GUILayout.Width(100))) {
					_clearData = true;
				}
			}
			_timelineView.Draw(_timelineData);
		}
	}
}
