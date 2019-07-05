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
using System.Reflection;
using UnityEngine;

namespace IPTech.Coroutines {
	public partial class YieldInstructionsFactory : IYieldInstructionFactory {

        protected class WaitForSecondsRealtimeWrapper : IEnumerator {
			readonly DateTime timeoutTime;

			public WaitForSecondsRealtimeWrapper(WaitForSecondsRealtime waitForSeconds) {
				FieldInfo fi = waitForSeconds.GetType().GetField("waitTime", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
				float waitTime = (float)fi.GetValue(waitForSeconds);
				float seconds = waitTime - Time.realtimeSinceStartup;

				timeoutTime = DateTime.Now.AddSeconds(seconds);
			}

            #region IEnumerator implementation

            public bool MoveNext() {
				return DateTime.Now < timeoutTime;
            }

            public void Reset() { }

            public object Current {
                get {
                    return null;
                }
            }

            #endregion
        }
    }
}

