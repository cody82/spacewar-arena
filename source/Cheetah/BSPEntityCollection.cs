using System;
using System.Collections;

namespace Cheetah
{
	/// <summary>
	/// Summary description for EntityCollection.
	/// </summary>
	public class BSPEntityCollection : ICollection
	{
		private ArrayList AllEntities = new ArrayList();

		public BSPEntityCollection()
		{
			//Do nothing
		}

		public BSPEntityCollection(String EntityString)
		{
			//Parse
			int StartBracePos = EntityString.IndexOf("{");

			while (StartBracePos >= 0)
			{
				int EndBracePos = EntityString.IndexOf("}", StartBracePos + 1);

				if (EndBracePos > -1)
				{
					String ArgValues = EntityString.Substring(StartBracePos + 1, 
						EndBracePos - StartBracePos - 1);

					AddEntity( new BSPEntity(ArgValues) );

					StartBracePos = EntityString.IndexOf("{", EndBracePos + 1);
				}
				else
				{
					StartBracePos = -1;
				}
			}
		}

		public void AddEntity(BSPEntity NewEntity)
		{
			AllEntities.Add(NewEntity);
		}

		public BSPEntity[] SeekEntitiesByClassname(String Classname)
		{
			ArrayList ReturnEntities = new ArrayList();

			foreach(BSPEntity CurrentEntity in AllEntities)
			{
				foreach(ArgValue CurrentArgValue in CurrentEntity)
				{
					if (CurrentArgValue.Argument == "classname")
					{
						if (CurrentArgValue.Value == Classname)
						{
							ReturnEntities.Add(CurrentEntity);
						}
					}
				}
			}

			return( (BSPEntity[])ReturnEntities.ToArray(typeof(BSPEntity)) );
		}

		public String SeekFirstEntityValue(String Classname, String Argument)
		{
			String ReturnValue = "";
			BSPEntity[] Entities = SeekEntitiesByClassname(Classname);

			if (Entities.Length > 0)
			{
				String[] Values = Entities[0].SeekValuesByArgument(Argument);

				if (Values.Length > 0)
				{
					ReturnValue = Values[0];
				}
			}

			return(ReturnValue);
		}

		#region ICollection Members

		public bool IsSynchronized
		{
			get
			{
				// TODO:  Add BSPEntityCollection.IsSynchronized getter implementation
				return AllEntities.IsSynchronized;
			}
		}

		public int Count
		{
			get
			{
				// TODO:  Add BSPEntityCollection.Count getter implementation
				return AllEntities.Count;
			}
		}

		public void CopyTo(Array array, int index)
		{
			AllEntities.CopyTo(array, index);
		}

		public object SyncRoot
		{
			get
			{
				// TODO:  Add BSPEntityCollection.SyncRoot getter implementation
				return AllEntities.SyncRoot;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			// TODO:  Add BSPEntityCollection.GetEnumerator implementation
			return AllEntities.GetEnumerator();
		}

		#endregion
	}
}
