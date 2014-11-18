//////////////////////////////////////////////////////////////////////
// Algorithmia is (c) 2014 Solutions Design. All rights reserved.
// https://github.com/SolutionsDesign/Algorithmia
//////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c) 2014 Solutions Design. All rights reserved.
// 
// The Algorithmia library sourcecode and its accompanying tools, tests and support code
// are released under the following license: (BSD2)
// ----------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met: 
//
// 1) Redistributions of source code must retain the above copyright notice, this list of 
//    conditions and the following disclaimer. 
// 2) Redistributions in binary form must reproduce the above copyright notice, this list of 
//    conditions and the following disclaimer in the documentation and/or other materials 
//    provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY SOLUTIONS DESIGN ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL SOLUTIONS DESIGN OR CONTRIBUTORS BE LIABLE FOR 
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
// BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
// USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
//
// The views and conclusions contained in the software and documentation are those of the authors 
// and should not be interpreted as representing official policies, either expressed or implied, 
// of Solutions Design. 
//
//////////////////////////////////////////////////////////////////////
// Contributers to the code:
//		- Frans  Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using SD.Tools.Algorithmia.UtilityClasses;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Class which limits calls to a method by using a timer which effectively calls the method instead. To limit calls to a given method, 
	/// pass it as a lambda to the method <see cref="Call(Action, double, ISynchronizeInvoke)"/>. The first time <see cref="Call(Action, double, ISynchronizeInvoke)"/> is called, a timer is started and the lambda
	/// is stored. When the timer elapses after the specified number of milliseconds, the stored lambda is called and the timer is reset, though
	/// not started again. When <see cref="Call(Action, double, ISynchronizeInvoke)"/> is called while the timer is running (so between the first call and the moment the timer elapses)
	/// the call is ignored. This way you can limit calls to a given method to 1 per interval. This is useful for situation where you have a lot of 
	/// events coming from different sources which all result in a single call to a given method. 
	/// </summary>
	/// <remarks>Be aware that if no synchronizationcontext is specified, the call to the lambda specified to <see cref="Call(Action, double, ISynchronizeInvoke)"/> will be done on
	/// another thread, namely the one the timer runs on</remarks>
	public class CallLimiter
	{
		#region Members
		private Timer _timer;
		private object _semaphore;
		private Action _toCall;
		private bool _eventHandled; // for race condition due to queued Elapsed event handling on threadpool thread. 
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="CallLimiter"/> class.
		/// </summary>
		public CallLimiter()
		{
			_semaphore = new object();
			_timer = new Timer() { AutoReset = false, Enabled = false };
			_timer.Elapsed += _timer_Elapsed;
		}


		/// <summary>
		/// Calls the specified to call after 500ms milliseconds, unless a call is already in progress, in which case the call is ignored. It doesn't matter if
		/// this method was called previously with a different toCall value. 
		/// </summary>
		/// <param name="toCall">The func to call.</param>
		/// <remarks>Calls to toCall will be made on a threadpool thread</remarks>
		public bool Call(Action toCall)
		{
			return Call(toCall, 500.0, null);
		}


		/// <summary>
		/// Calls the specified to call after intervalMS milliseconds, unless a call is already in progress, in which case the call is ignored. It doesn't matter if
		/// this method was called previously with a different toCall value.
		/// </summary>
		/// <param name="toCall">The func to call.</param>
		/// <param name="intervalMS">The interval to wait, in ms, before toCall is called. If interval is below 100, it's clamped to 100.</param>
		/// <returns>true if the call is accepted and will be performed in intervalMS ms. False if the call is ignored if a timer is 
		/// already in progress.</returns>
		/// <remarks>Calls to toCall will be made on a threadpool thread</remarks>
		public bool Call(Action toCall, double intervalMS)
		{
			return Call(toCall, intervalMS, null);
		}


		/// <summary>
		/// Calls the specified to call after intervalMS milliseconds, unless a call is already in progress, in which case the call is ignored. It doesn't matter if
		/// this method was called previously with a different toCall value.
		/// </summary>
		/// <param name="toCall">The func to call.</param>
		/// <param name="intervalMS">The interval to wait, in ms, before toCall is called. If interval is below 100, it's clamped to 100.</param>
		/// <param name="synchronizingObject">The synchronizing object. If null, the call to the lambda specified to <see cref="Call(Action, double, ISynchronizeInvoke)"/> might be done on a different thread, as the System
		/// timer used is using a threadpool in that case. If the call has to be the same thread as the UI, specify a UI object
		/// as synchronizingObject, e.g. a form object or control object on which this limiter is used.</param>
		/// <returns>true if the call is accepted and will be performed in intervalMS ms. False if the call is ignored if a timer is 
		/// already in progress.</returns>
		public bool Call(Action toCall, double intervalMS, ISynchronizeInvoke synchronizingObject)
		{
			ArgumentVerifier.CantBeNull(toCall, "toCall");
			lock(_semaphore)
			{
				if(_timer.Enabled)
				{
					// already in progress
					return false;
				}
				_eventHandled = false;
				_toCall = toCall;
				_timer.SynchronizingObject = synchronizingObject;
				_timer.Interval = Math.Max(100.0, intervalMS);
				_timer.Enabled = true;
				return true;
			}
		}


		/// <summary>
		/// Handles the Elapsed event of the timer object.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock(_semaphore)
			{
				if(_eventHandled)
				{
					// ignore this call, as it's a call due to threadpool queueing, see Timer class docs.
					return;
				}
				_eventHandled = true;
				_timer.Enabled = false;
				// if a synchronization object was specified, this event handler is called on the UI thread, otherwise this event handler
				// is called on a threadpool thread.
				_toCall();
			}
		}
	}
}
