/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

namespace IPTech.Coroutines.Examples {

	public class ExampleSingleton {
		static ICoroutineRunner _coroutineRunner;

		public static ICoroutineRunner Inst {
			get {
				if(_coroutineRunner == null) {
					_coroutineRunner = new CoroutineRunner("ExampleSingletonRunner");
				}
				return _coroutineRunner;
			}
		}
	}
}
