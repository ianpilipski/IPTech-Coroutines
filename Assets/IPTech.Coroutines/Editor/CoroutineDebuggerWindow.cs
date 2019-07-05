/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using System.Collections;
using UnityEditor;
using UnityEngine;

namespace IPTech.Coroutines.Insights {
	public class CoroutineDebuggerWindow : EditorWindow {
		ICoroutineRunner _runner;
		TimelineView _timelineView;
		TimelineData _timelineData;
		UpdateMode _updateMode = UpdateMode.PlayMode;

		public GUISkin TimelineSkin;

		public enum UpdateMode {
			PlayMode,
			Editor
		}

		[MenuItem("IPTech/Coroutine Debugger")]
		[MenuItem("Window/IPTech/Coroutine Debugger")]
		public static void ShowCoroutineDebuggerWindow() {
			CoroutineDebuggerWindow cdw = EditorWindow.GetWindow<CoroutineDebuggerWindow>("Coroutine Debugger");
			cdw.Show();
		}
	
		void OnEnable() {
			_timelineData = new TimelineData();
			_timelineView = new TimelineView(LoadSkin());
			wantsMouseMove = true;
			CFunc.CoroutineCreatedListeners += HandleCoroutineCreated;
			_runner = new CoroutineRunner("CoroutineDebuggerWindow");
			_runner.Start(UpdateCoroutine());
		}

		GUISkin LoadSkin() {
			return TimelineSkin;
		}

		void HandleCoroutineCreated(ICFunc obj) {
			_timelineData.Add(obj);
		}

		void OnDisable() {
			CFunc.CoroutineCreatedListeners-= HandleCoroutineCreated;
			_runner.Dispose();
			_runner = null;
		}

		IEnumerator UpdateCoroutine() {
			while(true) {
				if(_updateMode==UpdateMode.Editor || Application.isPlaying) {
					_timelineData.Update();
					Repaint();
				}
				yield return null;
			}
		}
		
		void OnGUI() {
			using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
				if(GUILayout.Button("Clear", EditorStyles.toolbarButton)) {
					EditorApplication.delayCall += () => {
						_timelineData.Clear();
						Repaint();
					};
				}
				GUILayout.FlexibleSpace();
				_updateMode = (UpdateMode)EditorGUILayout.EnumPopup(_updateMode, EditorStyles.toolbarDropDown);
			}
			_timelineView.Draw(_timelineData);
		}
	}
}