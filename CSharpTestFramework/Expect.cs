using System;
namespace CSharpTestFramework
{
	public class ExpectationException : ApplicationException
	{
		public ExpectationException(string message) : base(message) { }
	}
	
	public class Expect
	{
		public static void That(bool condition)
		{
			// TODO: Turn this debugging exception message into a feature
			if (condition == false)
				throw new Exception("Condition was false");
		}
		
		
		// Expect.That(1, Is.EqualTo(2));
		public static void That(object actual, object matcher)
		{
			if (!actual.Equals(matcher))
			{
				throw new ExpectationException(String.Format(
					"Expected {0} \"{1}\" to be equal to {2} \"{3}\", but it was not", actual.GetType().ToString(), actual, matcher.GetType().ToString(), matcher
				));
			}
		}
	}
	
	public class Is
	{
		public static object EqualTo(object other)
		{
			return other;
		}
	}
}

