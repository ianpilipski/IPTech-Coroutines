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
	public interface ICoroutineRunner : IEnumerable<ICFunc>, IDisposable
	{
		ICFunc Start(IEnumerator coroutine);
		ICFunc Start(IEnumerator coroutine, IYieldInstructionFactory yieldInstructionFactory);
		ICFunc Start(Func<IEnumerator> func);
		ICFunc Start(Func<IEnumerator> func, IYieldInstructionFactory yieldInstructionFactory);
		void Stop(ICFunc coroutine);
		ICFunc this[int index] { get; }
		int Count { get; }
	}
}

