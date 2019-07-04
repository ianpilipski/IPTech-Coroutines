/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using UnityEngine;
using System.Collections;

namespace IPTech.Coroutines.Examples {
	public class ExampleCreatingTheRunner : MonoBehaviour {

		string message;
		ICFunc _coroutine;
		CoroutineRunner _coroutineRunner;

		void Start() {
			message = "";
			_coroutineRunner = new CoroutineRunner("ExamplePrivateRunner");
		}

		bool IsCoroutineRunning {
			get {
				return _coroutine != null && !_coroutine.IsDone;
			}
		}

		IEnumerator ShowHelloWorldCoroutine() {
			yield return UpdateUICoroutine();
		}

		IEnumerator UpdateUICoroutine() {
			message = "Hello";
			yield return new WaitForSeconds(2);
			message = "World!";
			yield return new WaitForSeconds(2);
			yield return DrawEllipses();
			message = "Hello, World!";
			yield return new WaitForSeconds(2);
		}

		IEnumerator DrawEllipses() {
			message = ".";
			yield return new WaitForSeconds(0.25f);
			message = "..";
			yield return new WaitForSeconds(0.25f);
			message = "...";
			yield return new WaitForSeconds(0.25f);
		}


		void OnGUI() {
			DrawMenu();
			DrawMessage();
		}

		void DrawMenu() {
			Rect r = new Rect(10, 10, 200, 100);
			using(new GUILayout.AreaScope(r)) {
				GUI.enabled = !IsCoroutineRunning;
				if(GUILayout.Button("Start Using Example Singleton")) {
					_coroutine = ExampleSingleton.Inst.Start(ShowHelloWorldCoroutine());
				}
				if(GUILayout.Button("Start Using Utility Singleton")) {
					_coroutine = CoroutineUtility.Start(ShowHelloWorldCoroutine());
				}
				if(GUILayout.Button("Start Using Private Runner")) {
					_coroutineRunner.Start(ShowHelloWorldCoroutine());
				}
				GUI.enabled = true;
			}
		}

		void DrawMessage() {
			Rect r = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100);
			using(new GUILayout.AreaScope(r)) {
				using(new GUILayout.VerticalScope(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
					GUILayout.FlexibleSpace();
					using(new GUILayout.HorizontalScope(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))) {
						GUILayout.FlexibleSpace();
						GUILayout.Label(message);
						GUILayout.FlexibleSpace();
					}
					GUILayout.FlexibleSpace();
				}
			}
		}
	}
}
