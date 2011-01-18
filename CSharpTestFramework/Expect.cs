using System;
namespace CSharpTestFramework
{
	public class Expect
	{
		public static void That(bool condition)
		{
			if (condition == false)
				throw new Exception();
		}
	}
}

