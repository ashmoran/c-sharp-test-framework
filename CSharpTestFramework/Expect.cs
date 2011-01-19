using System;
using System.Dynamic;

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
			if (matcher.GetType() == typeof(ContainsMatcher)) {
				var ourMatcher = (ContainsMatcher)matcher;
				ourMatcher.Match(actual);
			}
			else if (!actual.Equals(matcher))
			{
				throw new ExpectationException(String.Format(
					"Expected {0} \"{1}\" to be equal to {2} \"{3}\", but it was not", actual.GetType().ToString(), actual, matcher.GetType().ToString(), matcher
				));
			}
		}
	}
	
	class ContainsMatcher
	{
		object m_contained;
		
		public ContainsMatcher(object contained)
		{
			m_contained = contained;
		}
		
		public void Match(dynamic actual)
		{
			Console.WriteLine(actual);
			Console.WriteLine(m_contained);
			// TODO: Figure out how to do this without a cast
			if (!actual.Contains((string)m_contained))
			{
				throw new ExpectationException(String.Format(
					"Expected {0} \"{1}\" to contain {2} \"{3}\", but it did not", actual.GetType().ToString(), actual, m_contained.GetType().ToString(), m_contained
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
	
	public class Contains
	{
		public static object Value(object contained)
		{
			return new ContainsMatcher(contained);
		}
	}
}

