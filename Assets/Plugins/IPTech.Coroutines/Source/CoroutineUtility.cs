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
using System.Collections.Generic;

namespace IPTech.Coroutines {

	///<summary>Static class utility for using the default coroutine runner as a singleton.</summary>
	///<remarks>
	///I recommend that you do not use singletons in your code, as it leads to rigid structures that are
	///not easily testable. I prefer controlled instantiation through assignment and constructors.
	///</remarks>
	public static class CoroutineUtility {
		static ICoroutineRunner _inst;

		static ICoroutineRunner Inst {
			get {
				if(_inst==null) {
					_inst = new CoroutineRunner();
				}
				return _inst;
			}
		}

		public static ICFunc Start(IEnumerator coroutine) {
			return Inst.Start(coroutine);
		}

		public static ICFunc Start(IEnumerator coroutine, IYieldInstructionFactory yieldInstructionFactory) {
			return Inst.Start(coroutine, yieldInstructionFactory);
		}

		public static ICFunc Start(Func<IEnumerator> func) {
			return Inst.Start(func);
		}

		public static ICFunc Start(Func<IEnumerator> func, IYieldInstructionFactory yieldInstructionFactory) {
			return Inst.Start(func, yieldInstructionFactory);
		}

		public static void Stop(ICFunc coroutine) {
			Inst.Stop(coroutine);
		}

		public static ICFunc GetCoroutine(int index) {
			return Inst[index];
		}

		public static int Count {
			get {
				return Inst.Count;
			}
		}

		public static IEnumerable<ICFunc> Coroutines {
			get {
				return Inst;
			}
		}
	}
}
