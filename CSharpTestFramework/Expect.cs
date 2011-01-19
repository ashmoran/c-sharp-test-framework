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
			if (!matcher.Match(actual))
			{
				throw new ExpectationException(matcher.FailureMessage);
			}
		}
	}
	
	public interface Matcher
	{
		public bool Match(dynamic actual);
		string FailureMessage { get; }
	}
	
	class ContainsMatcher : Matcher
	{
		object m_actual;
		object m_contained;
		
		public ContainsMatcher(object contained)
		{
			m_contained = contained;
		}
		
		public bool Match(dynamic actual)
		{
			m_actual = actual;
			
			// TODO: Figure out how to do this without a cast
			return actual.Contains((string)m_contained);
		}
		
		public string FailureMessage
		{
			get
			{
				return String.Format(
					"Expected {0} \"{1}\" to contain {2} \"{3}\", but it did not",
				    m_actual.GetType().ToString(), m_actual, m_contained.GetType().ToString(), m_contained
				);
			}
		}
	}
	
	public class IsEqualToMatcher : Matcher
	{
		object m_actual;
		object m_expected;
		
		public IsEqualToMatcher(object expected)
		{
			m_expected = expected;
		}
		
		public bool Match(dynamic actual)
		{
			m_actual = actual;
			return actual.Equals(m_expected);
		}
		
		public string FailureMessage
		{
			get
			{
				return String.Format(
					"Expected {0} \"{1}\" to be equal to {2} \"{3}\", but it was not",
				     m_actual.GetType().ToString(), m_actual, m_expected.GetType().ToString(), m_expected
				);
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

