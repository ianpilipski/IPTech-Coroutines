/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using System.Collections;
using UnityEngine;

namespace IPTech.Coroutines.Examples {
	public class ExampleCoroutineFunctions : MonoBehaviour {
		ICFunc _coroutineFunction;
		string startMessage;
		string endMessage;
		string description;

		
		void Example_Coroutine_WithError() {
			SetNewTest("You should see that the coroutine Ended message never appears");
			StartCoroutine(new CFunc(CoroutineWithError()));
		}

		void Example_Coroutine_WithCatch() {
			SetNewTest("You should see that the coroutine ended message now appears, because we handle it in a Catch");
			StartCoroutine(
				new CFunc(CoroutineWithError())
					.Catch(e => {
						endMessage = "CAUGHT EXCEPTION";
						Debug.LogWarning(e.ToString());
					})
			);
		}

		void Example_Coroutine_WithFinally() {
			SetNewTest("This shows that the finally handler will always be called, even if the coroutine has thrown an error.");
			StartCoroutine(
				new CFunc(CoroutineWithError()).Finally(() => {
					endMessage = "FINALLY CALLED";
				})
			);
		}

		void Example_Coroutine_WithCatchFinally() {
			SetNewTest("This shows that both the catch and finally handler will be called when a coroutine throws an exception.");
			StartCoroutine(
				new CFunc(CoroutineWithError()).Catch(e => {
					endMessage = "CAUGHT EXCEPTION,";
				}).Finally(() => {
					endMessage += "FINALLY CALLED,";
				})
			);
		}

		void Example_Coroutine_YieldingCoroutine() {
			SetNewTest("This shows that a coroutine could yield a coroutine function with all of the same functionality");
			StartCoroutine(CoroutineYieldingInnerCoroutine());
		}

		IEnumerator CoroutineYieldingInnerCoroutine() {
			yield return new CFunc(CoroutineWithError()).Catch(e => {
				endMessage = "CAUGHT EXCEPTION,";
			});
			endMessage += "FINISHED";
		}

		void Example_Checking_IsDone() {
			SetNewTest("This test shows how you can use the IsDone property to check if a coroutine is complete");
			CFunc coroutine = new CFunc(CoroutineWithError());
			StartCoroutine(coroutine);
			_coroutineFunction = coroutine;
		}

		IEnumerator CoroutineWithError() {
			startMessage = "STARTED";
			yield return new WaitForSecondsRealtime(1);
			yield return ThrowExceptionCoroutine();
			endMessage = "FINISHED";
		}

		void Update() {
			if(_coroutineFunction != null) {
				if(_coroutineFunction.IsDone) {
					if(_coroutineFunction.Error != null) {
						endMessage = "Coroutine Has Completed With Error";
					} else {
						endMessage = "Coroutine Has Completed";
					}
					_coroutineFunction = null;
				}
			}
		}

		void Start() {
			ClearMessages();
		}

		void SetNewTest(string desc) {
			description = desc;
			ClearMessages();
		}

		void ClearMessages() {
			startMessage = "";
			endMessage = "";
		}

		IEnumerator ThrowExceptionCoroutine() {
			yield return null;
			throw new System.Exception("Test Exception");
		}

		IEnumerator CoroutineWithInternalErrorHandling() {
			startMessage = "STARTED";
			yield return new WaitForSecondsRealtime(1);

			//we can yield it with catch or wait on it and check errors
			//we will use an empty catch to completely swallow the error
			yield return new CFunc(ThrowExceptionCoroutine()).Catch(e => { });

			endMessage = "FINISHED";

			yield return new WaitForSecondsRealtime(1);

			//Here we will use our own wait and check the error manually. then we don't need a catch block.
			//The reason it does not buble the error is because we don't yield the ICFunc
			startMessage = "STARTED ANOTHER";

			ICFunc coroFunc2 = CoroutineUtility.Start(ThrowExceptionCoroutine());
			while(!coroFunc2.IsDone) {
				yield return null;
			}
			if(coroFunc2.Error != null) {
				Debug.Log("We should have already seen an error log, but here is another..");
			}

			endMessage = "FINISHED ANOTHER";
		}

		private void OnGUI() {
			DrawOptions();
			DrawOutput();
		}

		void DrawOptions() {
			if(GUILayout.Button("Coroutine With Error")) {
				Example_Coroutine_WithError();
			}
			if(GUILayout.Button("Coroutine With Catch")) {
				Example_Coroutine_WithCatch();
			}
			if(GUILayout.Button("Coroutine With Finally")) {
				Example_Coroutine_WithFinally();
			}
			if(GUILayout.Button("Coroutine With Catch/Finally")) {
				Example_Coroutine_WithCatchFinally();
			}
			if(GUILayout.Button("Checking Coroutine IsDone")) {
				Example_Checking_IsDone();
			}
			if(GUILayout.Button("Coroutine yielding Coroutine")) {
				Example_Coroutine_YieldingCoroutine();
			}
		}

		void DrawOutput() {
			Rect r = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200);
			using(new GUILayout.AreaScope(r)) {
				using(new GUILayout.VerticalScope(GUI.skin.box, GUILayout.ExpandHeight(true))) {
					GUILayout.Label(description);
					GUILayout.FlexibleSpace();
					GUILayout.Label("Start: " + startMessage);
					GUILayout.Label("End: " + endMessage);
					GUILayout.FlexibleSpace();
				}
			}
		}
	}
}
