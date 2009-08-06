/*
Copyright (c) 2007 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;

namespace Lidgren.Library.Network
{
	public enum CommonBandwidths : int
	{
		Modem56kbps = 7000,
		OneMbps = 125000,
		TwoMbps = 250000,
		T1 = 192000,
		TenMbps = 1250000,
		HundredMbps = 12500000,
	}

	/// <summary>
	/// Configuration for a certain connection
	/// </summary>
	public class NetConnectionConfiguration
	{
		// defaults
		public static NetOptimization DefaultOptimization = NetOptimization.Neutral;
		public static float DefaultConnectionTimeOut = 20.0f;
		public static float DefaultPingFrequency = 6.0f;
		public static int DefaultThrottleBytesPerSecond = (int)CommonBandwidths.OneMbps;

		public double ConnectionTimeOut;
		public int AveragePingSamples;
		public float PingFrequency;
		public int ThrottleBytesPerSecond;

		// reliability
		public float ResendFirstUnacknowledgedDelay;
		public float ResendSubsequentUnacknowledgedDelay;
		public int NumResendsBeforeFailing;
		public float ForceExplicitAckDelay;

		// debug lag/loss/dupes inducer settings
		public bool SimulateLagLoss;
		public float LossChance;
		public float LagDelayChance;
		public float LagDelayMinimum;
		public float LagDelayVariance;
		public float DuplicatedPacketChance;

		private NetConnection m_forConnection;
		private NetOptimization m_currentOptimization;

		public NetOptimization CurrentOptimization
		{
			get { return m_currentOptimization; }
			set { m_currentOptimization = value; }
		}

		public NetConnectionConfiguration(NetConnection forConnection)
		{
			if (forConnection == null)
				throw new ArgumentNullException("forConnection");

			m_forConnection = forConnection;
			forConnection.Configuration = this;

			ConnectionTimeOut = NetConnectionConfiguration.DefaultConnectionTimeOut;
			AveragePingSamples = 5;
			PingFrequency = NetConnectionConfiguration.DefaultPingFrequency;
			m_currentOptimization = NetConnectionConfiguration.DefaultOptimization;
			ThrottleBytesPerSecond = NetConnectionConfiguration.DefaultThrottleBytesPerSecond;

			// default lag/loss/dupe settings
			SimulateLagLoss = false;
			LossChance = 0.05f;
			LagDelayChance = 1.0f;
			LagDelayMinimum = 0.1f;
			LagDelayVariance = 0.05f;
			DuplicatedPacketChance = 0.01f;

			// reliability
			OptimizeSettings(m_currentOptimization, 0.1f, true); // assume 100ms roundtrip for starters
		}

		internal void OptimizeSettings(float roundtrip)
		{
			OptimizeSettings(m_currentOptimization, roundtrip);
		}

		internal void OptimizeSettings(NetOptimization optimize, float roundtrip)
		{
			OptimizeSettings(optimize, roundtrip, false);
		}

		internal void OptimizeSettings(float roundtrip, bool silent)
		{
			OptimizeSettings(m_currentOptimization, roundtrip, silent);
		}

		internal void OptimizeSettings(NetOptimization optimize, float roundtrip, bool silent)
		{
			m_currentOptimization = optimize;

			if (!silent)
				m_forConnection.Parent.Log.Verbose("Optimizing network settings for " + NetUtil.SecToMil(roundtrip) + " ms roundtrip time (" + optimize.ToString() + ")");
			switch(optimize)
			{
				//
				// std = example of 100 ms roundtrip time
				//
				case NetOptimization.EmphasizeResponse:
					ResendFirstUnacknowledgedDelay = 0.05f + roundtrip * 1.5f;						// std: 
					ResendSubsequentUnacknowledgedDelay = 0.1f + (roundtrip * 3.0f);				// std: 
					NumResendsBeforeFailing = (int)(15.0f / ResendSubsequentUnacknowledgedDelay);	// std: 
					ForceExplicitAckDelay = roundtrip * 0.5f;										// std: 
					PingFrequency = 4.0f;
					break;
				case NetOptimization.EmphasizeBandwidth:
					ResendFirstUnacknowledgedDelay = 0.1f + roundtrip * 5.0f;						// std: 
					ResendSubsequentUnacknowledgedDelay = 0.25f + (roundtrip * 12.0f);				// std: 
					NumResendsBeforeFailing = (int)(30.0f / ResendSubsequentUnacknowledgedDelay);	// std: 
					ForceExplicitAckDelay = roundtrip * 2.0f;										// std: 
					PingFrequency = 12.0f;
					break;
				case NetOptimization.Neutral:
				default:
					ResendFirstUnacknowledgedDelay = 0.05f + roundtrip * 2.0f;						// std: 
					ResendSubsequentUnacknowledgedDelay = 0.25f + (roundtrip * 5.0f);				// std: 
					NumResendsBeforeFailing = (int)(20.0f / ResendSubsequentUnacknowledgedDelay);	// std: 
					ForceExplicitAckDelay = roundtrip * 1.0f;										// std: 
					PingFrequency = 6.0f;
					break;
			}

			m_forConnection.NotifyOptimized();
		}
	}
}
