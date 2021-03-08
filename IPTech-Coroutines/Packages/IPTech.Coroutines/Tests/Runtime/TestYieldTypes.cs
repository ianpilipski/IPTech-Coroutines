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
using UnityEngine.TestTools;

namespace IPTech.Coroutines {
	public class TestYieldTypes {

        [UnityTest]
        public IEnumerator AllKnownYieldTypesUsingRunner() {
            ICFunc cw = CoroutineUtility.Start(NewTestScriptWithEnumeratorPasses());
            yield return new WaitUntil(() => cw.IsDone);
        }

		[UnityTest]
		public IEnumerator AllKnownYieldTypesUsingUnityCoroutine() {
			//GameObject go = new GameObject("AllKnownYieldTypesUsingUnityCoroutine", typeof(TestYieldTypesMonoBehaviour));
			//var tmb = go.GetComponent<TestYieldTypesMonoBehaviour>();
			//tmb.StartUnityCoroutineTest(NewTestScriptWithEnumeratorPasses());
			yield return NewTestScriptWithEnumeratorPasses();
		}

		[UnityTest]
		public IEnumerator AllKnownYieldTypesUsingUnityCoroutineWithCoroutineFunction() {
			yield return new CFunc(NewTestScriptWithEnumeratorPasses());
		}

        public IEnumerator NewTestScriptWithEnumeratorPasses() {
            int i = 0;
            while (i++ < 10) {
                Debug.Log("I'm in my coroutine for step " + i);
                yield return null;
            }

            Debug.Log("I'm waiting for 5 seconds!");
            yield return new WaitForSeconds(5F);

            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                //This test doesn't seem to work in batchmode and headless on linux
                Debug.Log("I'm waiting until the end of frame");
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("I'm waiting for fixed update!");
            yield return new WaitForFixedUpdate();

            Debug.Log("I'm waiting for 5 seconds realtime!");
            yield return new WaitForSecondsRealtime(5F);

            Debug.Log("I'm waiting on custom WaitWhile!");
            i = 0;
            yield return new WaitWhile(() => {
                return i++ < 10;
            });

            Debug.Log("I'm waiting on a custom WaitUntil!");
            i = 0;
            yield return new WaitUntil(() => {
                return i++ > 10;
            });

#if !UNITY_2018_3_OR_NEWER
			Debug.Log("I'm waiting on an WWW request!");
            WWW www = new WWW("http://httpbin.org/get");
            yield return www;
#endif
            Debug.Log("I'm waiting on an async op!");
            ResourceRequest rr = Resources.LoadAsync<Object>("MyAsyncLoadObject");
            yield return rr;

            Debug.Log("I'm done!");
        }
    }
}
