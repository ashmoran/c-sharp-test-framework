using System;
namespace CSharpTestFramework
{
	public class Expect
	{
		public static void That(bool condition)
		{
			// TODO: Turn this debugging exception message into a feature
			if (condition == false)
				throw new Exception("Condition was false");
		}
	}
}

