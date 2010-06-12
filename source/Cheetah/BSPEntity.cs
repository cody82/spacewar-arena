using System;
using System.Collections;

namespace Cheetah.Quake
{
	/// <summary>
	/// Summary description for BSPEntity.
	/// </summary>

	public class ArgValue
	{
		public String Argument = "";
		public String Value = "";

		public ArgValue()
		{
			//Do nothing
		}

		public ArgValue(String Argument, String Value)
		{
			//Copy values
			this.Argument = Argument;
			this.Value = Value;
		}
	}

	public class BSPEntity : ICollection
	{
		private ArrayList ArgValues = new ArrayList();

		public BSPEntity()
		{
			//Do nothing
		}

		public BSPEntity(String EntityString)
		{
			//Parse
			bool IsArgument = true;
			ArgValue CurrentArgValue = new ArgValue();
			int StartQuotePos = EntityString.IndexOf("\"");

			while (StartQuotePos > -1)
			{
				int EndQuotePos = EntityString.IndexOf("\"", StartQuotePos + 1);

				if (EndQuotePos > -1)
				{
					String InnerText = EntityString.Substring(StartQuotePos + 1,
						EndQuotePos - StartQuotePos - 1);

					if (IsArgument)
					{
						CurrentArgValue = new ArgValue();
						CurrentArgValue.Argument = InnerText;
						IsArgument = false;
					}
					else
					{
						CurrentArgValue.Value = InnerText;
						AddArgValue(CurrentArgValue);
						IsArgument = true;
					}

					StartQuotePos = EntityString.IndexOf("\"", EndQuotePos + 1);
				}
				else
				{
					StartQuotePos = -1;
				}
			}
		}

		public void AddArgValue(String Argument, String Value)
		{
			ArgValues.Add( new ArgValue(Argument, Value) );
		}

		public void AddArgValue(ArgValue NewArgValue)
		{
			ArgValues.Add(NewArgValue);
		}

		public String[] SeekValuesByArgument(String Argument)
		{
			ArrayList ReturnValues = new ArrayList();

			foreach(ArgValue CurrentArgValue in ArgValues)
			{
				if (CurrentArgValue.Argument == Argument)
				{
					ReturnValues.Add(CurrentArgValue.Value);
				}
			}

			return( (String[])ReturnValues.ToArray(typeof(String)) );
		}

		public String SeekFirstValue(String Argument)
		{
			String ReturnString = "";
			String[] Values = SeekValuesByArgument(Argument);

			if (Values.Length > 0)
			{
				ReturnString = Values[0];
			}

			return(ReturnString);
		}

		#region ICollection Members

		public bool IsSynchronized
		{
			get
			{
				// TODO:  Add BSPEntity.IsSynchronized getter implementation
				return ArgValues.IsSynchronized;
			}
		}

		public int Count
		{
			get
			{
				// TODO:  Add BSPEntity.Count getter implementation
				return ArgValues.Count;
			}
		}

		public void CopyTo(Array array, int index)
		{
			ArgValues.CopyTo(array, index);
		}

		public object SyncRoot
		{
			get
			{
				// TODO:  Add BSPEntity.SyncRoot getter implementation
				return ArgValues.SyncRoot;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			// TODO:  Add BSPEntity.GetEnumerator implementation
			return ArgValues.GetEnumerator();
		}

		#endregion
	}
}
