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

namespace IPTech.Coroutines {
	public class CoroutineRunnerMonoBehaviour : MonoBehaviour, CoroutineRunner.IUpdater
	{
		Coroutine _coroOnUpdate;
		Coroutine _coroOnFixedUpdate;
		Coroutine _coroOnFrameUpdate;

		public Action Updated { get; set; }
		public Action FixedUpdateUpdated { get; set; }
		public Action EndOfFrameUpdated { get; set; }

		void Start() {
			_coroOnUpdate = StartCoroutine(OnUpdated());
			_coroOnFixedUpdate = StartCoroutine(OnFixedUpdateUpdated());
			_coroOnFrameUpdate = StartCoroutine(OnEndOfFrameUpdated());
		}
			
		IEnumerator OnFixedUpdateUpdated() {
			WaitForFixedUpdate wait = new WaitForFixedUpdate();
			while(true) {
				yield return wait;
				if(FixedUpdateUpdated != null) {
					try {
						FixedUpdateUpdated();
					} catch(Exception e) {
						Debug.LogException(e);
					}
				}
			}
		}

		IEnumerator OnEndOfFrameUpdated() {
			object wait = new WaitForEndOfFrame();
			while(true) {
				yield return wait;
				if(EndOfFrameUpdated != null) {
					try {
						EndOfFrameUpdated();
					} catch(Exception e) {
						Debug.LogException(e);
					}
				}
			}
		}
			
		IEnumerator OnUpdated() {
			while(true) {
				yield return null;
				if(Updated != null) {
					try {
						Updated();
					} catch(Exception e) {
						Debug.LogException(e);
					}
				}
			}
		}

		void OnDestroy() {
			if(_coroOnUpdate!=null) {
				StopCoroutine(_coroOnUpdate);
				_coroOnUpdate = null;
			}
			if(_coroOnFixedUpdate!=null) {
				StopCoroutine(_coroOnFixedUpdate);
				_coroOnFixedUpdate = null;
			}
			if(_coroOnFrameUpdate!=null) {
				StopCoroutine(_coroOnFrameUpdate);
				_coroOnFrameUpdate = null;
			}
			
			Updated = null;
			FixedUpdateUpdated = null;
			EndOfFrameUpdated = null;	
		}
		
		public void Dispose() {
			if(this!=null) {
				UnityEngine.Object.Destroy(this);
			}
		}
	}
}

