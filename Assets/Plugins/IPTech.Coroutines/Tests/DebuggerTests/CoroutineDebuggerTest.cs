/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using System;
using System.Collections;
using UnityEngine;

namespace IPTech.Coroutines.Tests {

	public class CoroutineDebuggerTest : MonoBehaviour {
	
		CoroutineRunner _coroutineRunner;
		
		void Start () {
			_coroutineRunner = new CoroutineRunner();
			
			for(int i=0;i<5;i++) { 
				_coroutineRunner.Start(ParentCoroutine(false));
				_coroutineRunner.Start(ParentCoroutine(true));
			}
		}
		
		IEnumerator ParentCoroutine(bool throwError) {
			yield return ChildCoroutine();
			yield return _coroutineRunner.Start(ChildCoroutine(true));
			yield return ChildCoroutine();
			if(throwError) throw new DebuggerTestException();
		}
		
		IEnumerator ChildCoroutine(bool throwError = false) {
			yield return new WaitForSeconds(5);
			yield return InnerCoroutine();
			if(throwError) throw new DebuggerTestException();
		}
		
		IEnumerator InnerCoroutine() {
			yield return null;
		}

		public class DebuggerTestException : Exception {
			public DebuggerTestException() : base("This is a test exception") { }
		}
	}
}
