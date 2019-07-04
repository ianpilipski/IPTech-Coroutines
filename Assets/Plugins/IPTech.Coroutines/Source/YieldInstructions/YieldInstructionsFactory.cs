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
	public partial class YieldInstructionsFactory : IYieldInstructionFactory
	{
		public YieldInstructionsFactory() {
		}

		public ICFunc CreateYieldInstructionWrapper(object yieldInstruction) {
			if(yieldInstruction is ICFunc) {
				return (ICFunc)yieldInstruction;
			}
			if(yieldInstruction is WaitForSeconds) {
				return new CFunc(new WaitForSecondsWrapper((WaitForSeconds)yieldInstruction));
			}
			if(yieldInstruction is AsyncOperation) {
				return new CFunc(new AsyncOperationWrapper((AsyncOperation)yieldInstruction));
			}
            if(yieldInstruction is WaitForSecondsRealtime) {
                return new CFunc(new WaitForSecondsRealtimeWrapper((WaitForSecondsRealtime)yieldInstruction));
            }
			if(yieldInstruction is IEnumerator) {
				return new CFunc((IEnumerator)yieldInstruction);
			}
			return null;
		}
	}
}

