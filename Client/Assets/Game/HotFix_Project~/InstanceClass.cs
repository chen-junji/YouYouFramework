using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hotfix
{
	public static class InstanceClass
	{

		public static void StaticFunTest(string a)
		{
			Debug.LogError("Unity调用了ILRuntime,然后打印出来了==" + a);
		}
	}
}
