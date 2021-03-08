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

        protected class WaitForSecondsWrapper : IEnumerator {
            readonly float seconds;
            readonly float timeOutTime;
            readonly DateTime start;

            public WaitForSecondsWrapper(WaitForSeconds waitForSeconds) {
                FieldInfo fi = waitForSeconds.GetType().GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                seconds = (float)fi.GetValue(waitForSeconds);
				if(Application.isPlaying) {
					timeOutTime = Time.time + seconds;
				} else {
					start = DateTime.Now;
				}
			}

            #region IEnumerator implementation

            public bool MoveNext() {
                if (Application.isPlaying) {
                    return Time.time <= timeOutTime;
                }
                return ((DateTime.Now - start)).TotalSeconds < seconds;
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

