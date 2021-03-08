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

namespace IPTech.Coroutines {
	public partial class YieldInstructionsFactory : IYieldInstructionFactory {

        protected class AsyncOperationWrapper : IEnumerator {
            AsyncOperation _asyncOp;

            public AsyncOperationWrapper(AsyncOperation asyncOp) {
                _asyncOp = asyncOp;
            }

            #region IEnumerator implementation

            public bool MoveNext() {
                return !_asyncOp.isDone;
            }

            public void Reset() {}

            public object Current {
				get {
					return null;
				}
			}

            #endregion
        }
    }
}

