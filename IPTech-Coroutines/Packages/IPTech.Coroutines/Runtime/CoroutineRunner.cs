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
using UnityEngine;
using Object = UnityEngine.Object;

namespace IPTech.Coroutines {
	public class CoroutineRunner : ICoroutineRunner, IDisposable {
		readonly List<ICFunc> _routines;
		IUpdater _updater;
		int _removedDuringStepCount;

		public CoroutineRunner() : this("IPTech.CoroutineRunner") { }

		public CoroutineRunner(string runnerName) {
			_routines = new List<ICFunc>();
			StartAutomaticUpdates(runnerName);
		}

		void StartAutomaticUpdates(string runnerName) {
			if(Application.isPlaying && _updater == null) {
				GameObject go = new GameObject(runnerName);
				Object.DontDestroyOnLoad(go);
				go.hideFlags = HideFlags.DontSave;
				_updater = (IUpdater)go.AddComponent<CoroutineRunnerMonoBehaviour>();
			} else {
				_updater = new UnityEditorUpdater();
			}

			_updater.Updated += StepRoutinesUpdate;
			_updater.FixedUpdateUpdated += StepRoutinesFixedUpdate;
			_updater.EndOfFrameUpdated += StepRoutinesEndOfFrame;
		}

		void StepRoutinesUpdate() {
			StepRoutines(ETickGroup.Update);
		}

		void StepRoutinesFixedUpdate() {
			StepRoutines(ETickGroup.FixedUpdate);
		}

		void StepRoutinesEndOfFrame() {
			StepRoutines(ETickGroup.EndOfFrame);
		}

		void StepRoutines(ETickGroup tickGroup) {
			_removedDuringStepCount = 0;
			for(int i = _routines.Count - 1; i >= _removedDuringStepCount; i--) {
				ICFunc cw = _routines[i - _removedDuringStepCount];
				if(cw.TickGroup == tickGroup) {
					if(MoveNextWithTryCatch(cw) == false) {
						RemoveFunction(cw);
					}
				}
			}
		}

		bool MoveNextWithTryCatch(ICFunc cFunc) {
			bool retVal = false;
			try {
				retVal = cFunc.MoveNext();
			} catch(Exception e) {
				UnityEngine.Debug.LogException(e);
			}
			return retVal;
		}

		#region ICoroutineRunner implementation

		public ICFunc Start(IEnumerator routine) {
			return AddFunction(new CFunc(routine));
		}

		public ICFunc Start(IEnumerator routine, IYieldInstructionFactory yieldInstructionFactory) {
			return AddFunction(new CFunc(routine, yieldInstructionFactory));
		}

		public ICFunc Start(Func<IEnumerator> func) {
			return AddFunction(new CFunc(func()));
		}

		public ICFunc Start(Func<IEnumerator> func, IYieldInstructionFactory yieldInstructionFactory) {
			return AddFunction(new CFunc(func(), yieldInstructionFactory));
		}

		ICFunc AddFunction(ICFunc function) {
			if(_routines.Contains(function)) { throw new AlreadyAddedCoroutineException(); }

			function.RegisterUpdater(this);
			_routines.Add(function);
			return function;
		}

		public void Stop(ICFunc routine) {
			_removedDuringStepCount++;
			RemoveFunction(routine);
		}

		void RemoveFunction(ICFunc function) {
			_routines.Remove(function);
		}

		public ICFunc this[int i] {
			get {
				return _routines[i];
			}
		}

		public int Count {
			get {
				return _routines.Count;
			}
		}
		#endregion

		#region IEnumerable implementation

		public System.Collections.Generic.IEnumerator<IPTech.Coroutines.ICFunc> GetEnumerator() {
			return _routines.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _routines.GetEnumerator();
		}

		#endregion

		public void Dispose() {
			//TODO: should error any running coroutines!
			if(_updater != null) {
				_updater.EndOfFrameUpdated = null;
				_updater.Updated = null;
				_updater.FixedUpdateUpdated = null;

				_updater.Dispose();
				_routines.Clear();
			}
		}

		public interface IUpdater : IDisposable {
			Action Updated { get; set; }
			Action FixedUpdateUpdated { get; set; }
			Action EndOfFrameUpdated { get; set; }
		}

		private class UnityEditorUpdater : IUpdater {
			public Action Updated { get; set; }
			public Action FixedUpdateUpdated { get; set; }
			public Action EndOfFrameUpdated { get; set; }

			public UnityEditorUpdater() {
				HookEditorUpdates();
			}

			void HookEditorUpdates() {
				UnhookEditorUpdates();
#if UNITY_EDITOR
				UnityEditor.EditorApplication.update += StepRoutinsEditor;
#endif
			}

			void UnhookEditorUpdates() {
#if UNITY_EDITOR
				UnityEditor.EditorApplication.update -= StepRoutinsEditor;
#endif
			}

			void StepRoutinsEditor() {
				if(Updated != null) Updated();
				if(FixedUpdateUpdated != null) FixedUpdateUpdated();
				if(EndOfFrameUpdated != null) EndOfFrameUpdated();
			}

			public void Dispose() {
				UnhookEditorUpdates();

				Updated = null;
				FixedUpdateUpdated = null;
				EndOfFrameUpdated = null;
			}
		}

		public class AlreadyAddedCoroutineException : Exception {
			public AlreadyAddedCoroutineException() : base("The coroutine is already registered with the coroutine runner") { }
		}
	}
}