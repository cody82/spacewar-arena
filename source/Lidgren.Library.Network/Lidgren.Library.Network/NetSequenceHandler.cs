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
using System.Text;

namespace Lidgren.Library.Network
{
	// todo: optimize this class
	internal class NetSequenceHandler
	{
		private int[] m_receivedSequences;
		private int[] m_expectedSequenceNumbers;
		private int[] m_receivedPtr;

		public NetSequenceHandler()
		{
			m_receivedSequences = new int[NetConstants.NumSequenceChannels * NetConstants.NumKeptDuplicateNumbers];
			for (int i = 0; i < m_receivedSequences.Length; i++)
				m_receivedSequences[i] = -1;
			m_receivedPtr = new int[NetConstants.NumSequenceChannels];
			m_expectedSequenceNumbers = new int[NetConstants.NumSequenceChannels];
		}

		public int RelateToExpected(NetMessage msg)
		{
			int expected = m_expectedSequenceNumbers[(int)msg.SequenceChannel];
			int diff = msg.SequenceNumber - expected;
			if (diff == 0)
				return 0; // early out
			// is a message so far behind that it's actually wrapped and early?
			if (diff < (NetConstants.EarlyArrivalWindowSize - NetConstants.NumSequenceNumbers))
				diff += NetConstants.NumSequenceNumbers;
			// is a message so far ahead that it's actually wrapped and late?
			else if (diff > NetConstants.EarlyArrivalWindowSize)
				diff -= NetConstants.NumSequenceNumbers;
			return diff;
		}

		public void AddSequence(NetMessage msg, out bool rejectMessage, out bool ackMessage, out bool withholdMessage)
		{
			int seqChan = (int)msg.SequenceChannel;
			int seqNr = msg.SequenceNumber;

			ackMessage = (seqChan >= (int)NetChannel.ReliableUnordered);
			
			int offset = seqChan * NetConstants.NumKeptDuplicateNumbers;
			int end = offset + NetConstants.NumKeptDuplicateNumbers;
			for (int i = offset; i < end; i++)
			{
				if (m_receivedSequences[i] == seqNr)
				{
					// duplicate
					NetBase.CurrentContext.Log.Verbose("Duplicate message " + msg + " detected and dropped");
					rejectMessage = true;
					withholdMessage = false;
					return;
				}
			}

			//NetBase.CurrentContext.Log.Verbose("Unique message " + msg + " found");

			// not duplicate; add!
			m_receivedSequences[offset + m_receivedPtr[seqChan]] = seqNr;
			m_receivedPtr[seqChan]++;
			if (m_receivedPtr[seqChan] >= NetConstants.NumKeptDuplicateNumbers)
				m_receivedPtr[seqChan] = 0;

			// unreliable?
			if (msg.SequenceChannel == NetChannel.Unreliable)
			{
				rejectMessage = false;
				withholdMessage = false;
				return;
			}

			int rel = RelateToExpected(msg);

			// sequenced?
			if (seqChan >= (int)NetChannel.Sequenced1 && seqChan <= (int)NetChannel.Sequenced15)
			{
				// late messages rejected if sequenced
				if (rel < 0)
				{
					NetBase.CurrentContext.Log.Debug("Late sequenced message " + msg.SequenceChannel + "|" + msg.SequenceNumber + " detected and dropped; expected " + m_expectedSequenceNumbers[seqChan] + " got " + msg.SequenceNumber);
					rejectMessage = true;
				}
				else
				{
					m_expectedSequenceNumbers[seqChan] = (m_expectedSequenceNumbers[seqChan] + rel + 1) % NetConstants.NumSequenceNumbers;
					rejectMessage = false;
				}
				withholdMessage = false;
				return;
			}

			// reliable unordered?
			if (msg.SequenceChannel == NetChannel.ReliableUnordered)
			{
				rejectMessage = false;
				withholdMessage = false;
				return;
			}

			// ordered

			if (rel > 0)
			{
				// early
				int expected = m_expectedSequenceNumbers[(int)msg.SequenceChannel];
				//NetBase.CurrentContext.Log.Debug("Received early message (" + msg.SequenceChannel + "|" + msg.SequenceNumber + ", expecting " + expected + ")");
				withholdMessage = true;
			}
			else if (rel == 0)
			{
				// on time!
				AdvanceExpected(msg.SequenceChannel, 1);
				withholdMessage = false;
			}
			else
			{
				// late, shouldn't happen!
				int expected = m_expectedSequenceNumbers[(int)msg.SequenceChannel];
				NetBase.CurrentContext.Log.Warning("Late ordered message " + msg + " received?! expecting " + expected + " received " + msg.SequenceNumber + " rel " + rel);
				withholdMessage = true;
			}
			
			rejectMessage = false;
			return;
		}

		internal void AdvanceExpected(NetChannel channel, int add)
		{
			int seqChan = (int)channel;
			int cur = m_expectedSequenceNumbers[seqChan];
			cur = (cur + add) % NetConstants.NumSequenceNumbers;
			m_expectedSequenceNumbers[seqChan] = cur;
		}
	}
}
