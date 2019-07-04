using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.Coroutines.Examples {
	public class EditorCoroutineRunner : EditorWindow {

		CoroutineRunner _runner;
		ICFunc _coroutine;

		[MenuItem("Window/IPTech/Examples/EditorCoroutine")]
		static void OpenEditorCoroutineRunnerWindow() {
			var window = EditorWindow.GetWindow<EditorCoroutineRunner>();
			window.Show();
		}

		private void OnEnable() {
			_runner = new CoroutineRunner();
		}

		private void OnDisable() {
			_runner.Dispose();
			_runner = null;
		}

		private void OnGUI() {
			if(_coroutine == null) {
				EditorGUILayout.HelpBox("There is currently no coroutine running.  Click the button to start a coroutine in the editor.", MessageType.None);
				if(GUILayout.Button("Start Coroutine")) {
					_coroutine = _runner.Start(MyCoroutine());
				}
			} else {
				EditorGUILayout.HelpBox("The coroutine is currently running, you can check the logs for the output.", MessageType.Info);
				if(GUILayout.Button("Stop Coroutine")) {
					_runner.Stop(_coroutine);
					_coroutine = null;
				}
			}
		}

		IEnumerator MyCoroutine() {
			int i = 0;
			while(true) {
				Debug.Log("I'm running step " + (i++));
				yield return new WaitForSecondsRealtime(1);
			}
		}
	}
}
