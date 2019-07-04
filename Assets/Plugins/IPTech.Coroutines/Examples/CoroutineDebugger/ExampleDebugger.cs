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
	public class ExampleDebugger : MonoBehaviour {

		void Start() {
			for(int i = 0; i < 10; i++) {
				CoroutineUtility.Start(ParentCoroutine("id=" + i));
			}
		}

		IEnumerator ParentCoroutine(string msg) {
			Debug.Log("ParentCoroutine: " + msg);
			yield return ChildCoroutine(msg + ".1");
			yield return ChildCoroutine(msg + ".2");
			yield return ChildCoroutine(msg + ".3");
		}

		IEnumerator ChildCoroutine(string msg) {
			Debug.Log("ChildCoroutine: " + msg);
			yield return new WaitForSeconds(5);
			yield return InnerCoroutine(msg);
		}

		IEnumerator InnerCoroutine(string msg) {
			Debug.Log("InnerCoroutine: " + msg);
			yield return null;
		}
	}
}
