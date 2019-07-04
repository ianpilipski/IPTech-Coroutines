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
using Debug = System.Diagnostics.Debug;

namespace IPTech.Coroutines {
	public class CFunc : ICFunc {
		enum ProcessResult {
			NeedsMoreProcessing,
			ProcessingComplete
		}

		IEnumerator _routine;
		ICFunc _subroutine;
		Action<Exception> _onErrorHandler;
		Action _onFinallyHandler;
		object _objectResponsibleForCallingUpdate;
		IYieldInstructionFactory _yieldInstructionFactory;
		
		public static event Action<ICFunc> CoroutineCreatedListeners;

		public bool IsDone { get; private set; }

		public Exception Error { get; private set; }

		public CFunc(Func<IEnumerator> func)
		: this(func(), Configuration.DefaultYieldInstructionFactory) {
		}

		public CFunc(Func<IEnumerator> func, IYieldInstructionFactory yieldInstructionFactory)
		: this(func(), yieldInstructionFactory) {
		}

		public CFunc(IEnumerator routine)
		: this(routine, Configuration.DefaultYieldInstructionFactory) {
		}

		public CFunc(IEnumerator routine, IYieldInstructionFactory yieldInstructionFactory) {
			_yieldInstructionFactory = yieldInstructionFactory;
			_routine = routine;
			_subroutine = null;
			FunctionName = routine.ToString();
			LastUpdated = DateTime.Now;
			OnCoroutineCreated();
		}

		public void RegisterUpdater(object updater) {
			if(!object.ReferenceEquals(_objectResponsibleForCallingUpdate, null)) throw new RunnerAlreadyDefined();

			_objectResponsibleForCallingUpdate = updater;
		}

		void OnCoroutineCreated() {
			if(CoroutineCreatedListeners != null) {
				CoroutineCreatedListeners(this);
			}
		}

		private void HandleError(Exception e) {
			if(_onErrorHandler == null) {
				//UnityEngine.Debug.LogException(e);
				Error = e;
				return;
			}

			try {
				_onErrorHandler(e);
			} catch(Exception ee) {
				//UnityEngine.Debug.LogException(ee);
				Error = ee;
			}
		}

		private void HandleFinally() {
			if(_onFinallyHandler != null) {
				try {
					_onFinallyHandler();
				} catch(Exception ee) {
					UnityEngine.Debug.LogException(ee);
					Error = ee;
				}
			}
		}

		public CFunc Catch(Action<Exception> onError) {
			Debug.Assert(_onErrorHandler == null, "An error handler was already registered");
			Debug.Assert(IsDone == false, "You can not register an error handler after the coroutine is completed");

			_onErrorHandler = onError;
			return this;
		}

		public CFunc Finally(Action onFinally) {
			Debug.Assert(_onFinallyHandler == null, "A finally handler was already registered");
			Debug.Assert(IsDone == false, "You can not register a finally handler after the coroutine is completed");

			_onFinallyHandler = onFinally;
			return this;
		}

		private void ExecuteNextStep() {
			LastUpdated = DateTime.Now;

			if(ProcessSubRoutine() == ProcessResult.NeedsMoreProcessing) return;

			if(_routine.MoveNext()) {
				return;
			}

			IsDone = true;
		}

		private ProcessResult ProcessSubRoutine() {
			if(_subroutine != null) {
				try {
					StepSubroutine();
					if(!_subroutine.IsDone) {
						return ProcessResult.NeedsMoreProcessing;
					}

					HandleSubRoutineDone();
				} catch(Exception e) {
					_subroutine = null;
					throw e;
				}
			}
			return ProcessResult.ProcessingComplete;
		}

		private void StepSubroutine() {
			if(_subroutine.IsUpdatedBy(this)) {
				_subroutine.MoveNext();
			}
		}

		void HandleSubRoutineDone() {
			Exception error = _subroutine.Error;
			_subroutine = null;
			if(error != null) {
				throw error;
			}
		}

		private void ProcessRoutineValue() {
			object val = _routine.Current;

			if(val == null) return;

			CreateSubRoutineForVal(val);
		}

		private void CreateSubRoutineForVal(object val) {
			_subroutine = _yieldInstructionFactory.CreateYieldInstructionWrapper(val);
			if(_subroutine != null && !_subroutine.HasUpdater) {
				_subroutine.RegisterUpdater(this);
			}
		}

		#region ICFunc implementation

		public bool HasUpdater { get { return !IsUpdatedBy(null); } }

		public bool IsUpdatedBy(object obj) {
			return object.ReferenceEquals(_objectResponsibleForCallingUpdate, obj);
		}

		public bool MoveNext() {
			if(IsDone) return false;

			bool tickAgain = true;
			while(tickAgain) {
				tickAgain = false;
				try {
					ExecuteNextStep();
				} catch(Exception e) {
					HandleError(e);
					IsDone = true;
				}

				if(IsDone) {
					HandleFinally();
					_routine = null;
					if(Error != null) {
						throw Error;
					}
					return false;
				}

				if(_subroutine == null) {
					ProcessRoutineValue();
					tickAgain = _subroutine != null;
				}
			}
			return true;
		}

		public ETickGroup TickGroup {
			get {
				if(_subroutine != null && _subroutine.IsUpdatedBy(this)) {
					return _subroutine.TickGroup;
				}
				object curVal = Current;
				if(curVal is WaitForEndOfFrame) {
					return ETickGroup.EndOfFrame;
				} else if(curVal is WaitForFixedUpdate) {
					return ETickGroup.FixedUpdate;
				}
				return ETickGroup.Update;
			}
		}

		public void Reset() {
			// do nothing
		}

		public object Current {
			get {
				if(_subroutine != null) return null;
				return _routine != null ? _routine.Current : null;
			}
		}

		#endregion

		#region ICFuncInsights
		public string FunctionName {
			get; private set;
		}

		public DateTime LastUpdated {
			get; private set;
		}
		#endregion

		public class RunnerAlreadyDefined : Exception {
			public RunnerAlreadyDefined() : base("Can not register a new owning coroutine runner, one is already defined.") { }
		}

		public static class Configuration {
			public static IYieldInstructionFactory DefaultYieldInstructionFactory = new YieldInstructionsFactory();
		}
	}
}

