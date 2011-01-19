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
				throw new ExpectationException("Condition was false");
		}
		
		public static void That(object actual, Matcher matcher)
		{
			matcher.Match(actual);
		}
	}
	
	public interface Matcher
	{
		public void Match(dynamic actual);
	}
	
	class ContainsMatcher : Matcher
	{
		object m_contained;
		
		public ContainsMatcher(object contained)
		{
			m_contained = contained;
		}
		
		public void Match(dynamic actual)
		{
			// TODO: Figure out how to do this without a cast
			if (!actual.Contains((string)m_contained))
			{
				throw new ExpectationException(String.Format(
					"Expected {0} \"{1}\" to contain {2} \"{3}\", but it did not", actual.GetType().ToString(), actual, m_contained.GetType().ToString(), m_contained
				));
			}
		}
	}
	
	public class IsEqualToMatcher : Matcher
	{
		object m_expected;
		
		public IsEqualToMatcher(object expected)
		{
			m_expected = expected;
		}
		
		public void Match(dynamic actual)
		{
			if (!actual.Equals(m_expected))
			{
				throw new ExpectationException(String.Format(
					"Expected {0} \"{1}\" to be equal to {2} \"{3}\", but it was not", actual.GetType().ToString(), actual, m_expected.GetType().ToString(), m_expected
				));
			}
		}
	}
	
	public class Is
	{
		public static Matcher EqualTo(object expected)
		{
			return new IsEqualToMatcher(expected);
		}
	}
	
	public class Contains
	{
		public static Matcher Value(object contained)
		{
			return new ContainsMatcher(contained);
		}
	}
}

