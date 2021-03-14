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
using NUnit.Framework;
using UnityEngine.TestTools;

namespace IPTech.Coroutines {
	public class TestCoroutineRunnerPlayMode {
		bool goalReachedTestStopRoutine;
		bool goal2ReachedTestStopRoutine;

		CoroutineRunner runnerUnderTest;
		ICFunc mockCoroutine;
		int moveNextCallCount;
		ETickGroup mockTickGroup;

		[SetUp]
		public void SetUp() {
			CoroutineRunner cr = new CoroutineRunner();
			runnerUnderTest = cr;

			moveNextCallCount = 0;
			mockTickGroup = ETickGroup.Update;
			mockCoroutine = new MockCoroutine(this);
		}

		public class MockCoroutine : ICFunc {
			TestCoroutineRunnerPlayMode runner;

			public MockCoroutine(TestCoroutineRunnerPlayMode runner) {
				this.runner = runner;
			}

			public bool IsDone => false;

			public Exception Error => null;

			public bool HasUpdater => false;

			public ETickGroup TickGroup => runner.mockTickGroup;

			public string FunctionName => "testfunc";

			public DateTime LastUpdated => DateTime.Now;

			public object Current => null;

			public bool IsUpdatedBy(object obj) {
				return false;
			}

			public bool MoveNext() {
				return runner.moveNextCallCount++ < 10;
			}

			public void RegisterUpdater(object runner) {
				
			}

			public void Reset() {
				
			}
		}

		[TearDown]
		public void TearDown() {
			runnerUnderTest.Dispose();
		}

		[UnityTest]
		public IEnumerator Start_calls_movenext_on_coroutine() {
			runnerUnderTest.Start(mockCoroutine);
			int i = 10;
			while(i-- > 0) {
				yield return null;
			}
			Assert.AreEqual(10, moveNextCallCount);
		}

		[Test]
		public void Start_increments_count() {
			runnerUnderTest.Start(TestRoutine());
			Assert.AreEqual(1, runnerUnderTest.Count);
			runnerUnderTest.Start(TestRoutine());
			Assert.AreEqual(2, runnerUnderTest.Count);
		}

		[UnityTest]
		public IEnumerator Stop_stops_coroutine() {
			ICFunc cw = null;
			cw = runnerUnderTest.Start(TestStopRoutine(() => {
				runnerUnderTest.Stop(cw);
			}));
			int i = 100;
			while(i-- > 0) {
				yield return null;
			}
			Assert.IsTrue(goalReachedTestStopRoutine);
			Assert.IsFalse(goal2ReachedTestStopRoutine);
		}

		[Test]
		public void Stop_decreases_count() {
			ICFunc cw = runnerUnderTest.Start(TestRoutine());
			ICFunc cw2 = runnerUnderTest.Start(TestRoutine());
			runnerUnderTest.Stop(cw);
			Assert.AreEqual(1, runnerUnderTest.Count);
			runnerUnderTest.Stop(cw2);
			Assert.AreEqual(0, runnerUnderTest.Count);
		}

		[Test]
		public void Stop_removes_coroutine_from_indexer() {
			ICFunc cw = runnerUnderTest.Start(TestRoutine());
			ICFunc cw2 = runnerUnderTest.Start(TestRoutine());
			runnerUnderTest.Stop(cw);
			Assert.AreSame(cw2, runnerUnderTest[0]);
		}

		[Test]
		public void Indexer_returns_coroutines_in_order_added() {
			ICFunc[] cwArray = new CFunc[10];
			for(int i = 0; i < cwArray.Length; i++) {
				cwArray[i] = runnerUnderTest.Start(TestRoutine());
			}
			for(int i = 0; i < cwArray.Length; i++) {
				Assert.AreSame(cwArray[i], runnerUnderTest[i]);
			}
		}

		[Test]
		public void IEnumerable_iteration_returns_coroutines_in_order_added() {
			ICFunc[] cwArray = new CFunc[10];
			for(int i = 0; i < cwArray.Length; i++) {
				cwArray[i] = runnerUnderTest.Start(TestRoutine());
			}
			int index = 0;
			foreach(CFunc cw in runnerUnderTest) {
				Assert.AreSame(cwArray[index++], cw);
			}
		}

		[UnityTest]
		public IEnumerator MoveNext_is_called_for_Update() {
			mockTickGroup = ETickGroup.Update;
			runnerUnderTest.Start(mockCoroutine);
			yield return null;
			yield return null;
			Assert.AreEqual(2, moveNextCallCount);
		}

		[UnityTest]
		public IEnumerator MoveNext_is_called_for_EndOfFrame() {
			mockTickGroup = ETickGroup.EndOfFrame;
			runnerUnderTest.Start(mockCoroutine);
			yield return null;
			yield return null;
			Assert.AreEqual(2, moveNextCallCount);
		}

		[UnityTest]
		public IEnumerator MoveNext_is_called_for_FixedUpdate() {
			mockTickGroup = ETickGroup.FixedUpdate;
			runnerUnderTest.Start(mockCoroutine);
			yield return null;
			yield return null;
			Assert.AreEqual(2, moveNextCallCount);
		}

		IEnumerator TestRoutine() {
			yield return null;
		}

		IEnumerator TestStopRoutine(Action action) {
			yield return null;
			goalReachedTestStopRoutine = true;
			action();
			yield return null;
			goal2ReachedTestStopRoutine = true;
		}
	}
}

