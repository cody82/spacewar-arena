using System;
using System.Collections.Generic;
using System.Text;

namespace Lidgren.Library.Network
{
	internal class NetStringTable
	{
		private List<string> m_data;

		public NetStringTable()
		{
			m_data = new List<string>();
		}

		/// <summary>
		/// Returns encoded index
		/// </summary>
		public int Write(NetMessage msg, string str)
		{
			int idx = m_data.IndexOf(str);
			if (idx == -1)
			{
				idx = m_data.Count;
				m_data.Add(str);

				msg.Write(false);
				msg.Write7BitEncodedUInt((uint)idx);
				msg.Write(str);
				return idx;
			}
			msg.Write(true);
			msg.Write7BitEncodedUInt((uint)idx);
			return idx;
		}

		/// <summary>
		/// Reuses the string table index
		/// </summary>
		public int Rewrite(NetMessage msg, int idx, string str)
		{
			while (m_data.Count < idx - 1)
				m_data.Add(null); // dummy values

			m_data[idx] = str;
			msg.Write(false);
			msg.Write7BitEncodedUInt((uint)idx);
			msg.Write(str);
			return idx;
		}

		public string Read(NetMessage msg)
		{
			bool encoded = msg.ReadBoolean();
			int idx = (int)msg.Read7BitEncodedUInt();
			string retval;
			if (!encoded)
			{
				// insert into table
				retval = msg.ReadString();
				while(m_data.Count <= idx)
					m_data.Add(null); // dummy values
				m_data[idx] = retval;
				return retval;
			}

			// look in table
			if (m_data.Count <= idx)
				return null;
			return m_data[idx];
		}
	}
}
