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
	public enum ETickGroup {
		Update,
		FixedUpdate,
		EndOfFrame
	}
		
	public interface ICFunc : IEnumerator
	{
		bool IsDone { get; }
		Exception Error { get; }
		bool HasUpdater { get; }
		bool IsUpdatedBy(object obj);
		ETickGroup TickGroup { get; }
		string FunctionName { get; }
		DateTime LastUpdated { get; }

		void RegisterUpdater(object runner);
	}
}

