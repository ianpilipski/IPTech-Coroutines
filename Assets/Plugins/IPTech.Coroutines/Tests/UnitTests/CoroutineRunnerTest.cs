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
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;
using IPTech.Coroutines;
using System.Collections.Generic;

namespace CoroutineRunnerTests {
	public class StartMethod : TestFixture {
		[UnityTest]
		public IEnumerator MoveNextIsCalledAsExpected(
			[Values] ETickGroup tickGroup,
			[Range(1, 10)] int callCount
		) {
			expectedCallCount = callCount;
			mockTickGroup = tickGroup;

			runnerUnderTest.Start(mockCoroutine);
			int i = 15; // more than enough to complete the coroutine
			while(i-- > 0) {
				yield return null;
			}

			mockCoroutine.Received(expectedCallCount).MoveNext();
		}

		[Test]
		public void CountIsIncremented() {
			for(int i = 1; i < 11; i++) {
				runnerUnderTest.Start(RoutineYieldNull());
				Assert.AreEqual(i, runnerUnderTest.Count);
			}
		}
	}

	public class StopMethod : TestFixture {
		[UnityTest]
		public IEnumerator StopCoroutine() {
			ICFunc cw = null;
			cw = runnerUnderTest.Start(RoutineTestStop(() => {
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
		public void CountIsDecremented() {
			List<ICFunc> list = new List<ICFunc>();
			for(int i = 1; i < 11; i++) {
				list.Add(runnerUnderTest.Start(RoutineYieldNull()));
			}
			Assume.That(runnerUnderTest.Count == 10);

			for(int i = 0; i < list.Count; i++) {
				runnerUnderTest.Stop(list[i]);
				Assert.AreEqual(list.Count - (i + 1), runnerUnderTest.Count);
			}
		}

		[Test]
		public void RemovesCoroutineFromIndexer() {
			ICFunc cw = runnerUnderTest.Start(RoutineYieldNull());
			ICFunc cw2 = runnerUnderTest.Start(RoutineYieldNull());
			Assume.That(runnerUnderTest[0] == cw);
			Assume.That(runnerUnderTest[1] == cw2);

			runnerUnderTest.Stop(cw);

			Assert.AreSame(cw2, runnerUnderTest[0]);
		}
	}

	public class IndexProperty : TestFixture {
		[Test]
		public void ReturnsCoroutineInOrderAdded() {
			ICFunc[] cwArray = new CFunc[10];
			for(int i = 0; i < cwArray.Length; i++) {
				cwArray[i] = runnerUnderTest.Start(RoutineYieldNull());
			}
			for(int i = 0; i < cwArray.Length; i++) {
				Assert.AreSame(cwArray[i], runnerUnderTest[i]);
			}
		}
	}

	public class IEnumerableInterface : TestFixture {
		[Test]
		public void IteratesCoroutinesInOrderAdded() {
			ICFunc[] cwArray = new CFunc[10];
			for(int i = 0; i < cwArray.Length; i++) {
				cwArray[i] = runnerUnderTest.Start(RoutineYieldNull());
			}
			int index = 0;
			foreach(CFunc cw in runnerUnderTest) {
				Assert.AreSame(cwArray[index++], cw);
			}
		}
	}

	public class TestFixture {
		protected int expectedCallCount;
		protected bool goalReachedTestStopRoutine;
		protected bool goal2ReachedTestStopRoutine;

		protected CoroutineRunner runnerUnderTest;
		protected ICFunc mockCoroutine;
		protected int moveNextCallCount;
		protected ETickGroup mockTickGroup;

		[SetUp]
		public void SetUp() {
			expectedCallCount = 10;
			runnerUnderTest = new CoroutineRunner();

			moveNextCallCount = 0;
			mockTickGroup = ETickGroup.Update;
			mockCoroutine = Substitute.For<ICFunc>();
			mockCoroutine.MoveNext().Returns(ci => ++moveNextCallCount < expectedCallCount);
			mockCoroutine.TickGroup.Returns(ci => mockTickGroup);
		}

		[TearDown]
		public void TearDown() {
			runnerUnderTest.Dispose();
		}

		protected IEnumerator RoutineYieldNull() {
			yield return null;
		}

		protected IEnumerator RoutineTestStop(Action action) {
			yield return null;
			goalReachedTestStopRoutine = true;
			action();
			yield return null;
			goal2ReachedTestStopRoutine = true;
		}
	}
}

