#if !NETCORE
using System;
using System.Security;
using System.Security.Permissions;

using CloneExtensions;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class PermissionSpec
	{
		public class Test
		{
			public int X { get; set; }

			private readonly object y = new object();

			// this field to make class unsafe and readonly
#pragma warning disable 169
			private readonly UnsafeStructTest z;
#pragma warning restore 169

			public object GetY()
			{
				return y;
			}
		}

		public struct UnsafeStructTest
		{
			public object Y { get; set; }
		}

		public class Executor : MarshalByRefObject
		{
			 public void DoDeepClone()
			 {
				 var test = new Test();
				 var clone = test.DeepClone();
				 if (clone.GetY() == test.GetY())
					 throw new Exception("Deep Clone fail");
				 
				 var clone2 = new Tuple<int>(12).DeepClone();
				 if (clone2.Item1 != 12)
					 throw new Exception("Deep Clone fail");
			 }

			public void DoShallowClone()
			 {
				 new Test().ShallowClone();
			 }

			public void CloneExtensionsClone()
			{
				new Test().GetClone();
			}
		}
	}
}
#endif