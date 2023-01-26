using System;
using System.Collections;
using System.Collections.Generic;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI; 

public class TranspositionTableWIP
{
	public Dictionary<long, object[]> TransTable;
	public TranspositionTableWIP()
	{
		TransTable = new Dictionary<long, object[]>();
	}

	public void Add(long key, int depth, string flag, float value) 
    {
		object[] values = new object[] {depth, flag, value};
		if(TransTable.ContainsKey(key)) TransTable[key]=values;
		else TransTable.Add(key, values);
    }

	public object[] GetValues(long key)
    {
		return TransTable[key];
    }

	public bool ContainsKey(long key)
    {
		return TransTable.ContainsKey(key);
    }

}