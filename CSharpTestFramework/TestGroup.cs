using System;
using System.Collections.Generic;
using System.Dynamic;

namespace CSharpTestFramework
{
	public delegate void Test();
	public delegate void ContextualTest(dynamic context);
	public delegate Object TestObjectExpression();

	public class TestGroup
	{
		// TODO: Extract and test independently
		public class TestContext : DynamicObject
		{
			// TODO: Name this type
			Dictionary<string, TestObjectExpression> m_letExpressions;
			
			Dictionary<string, object> m_evaluatedExpressions = new Dictionary<string, object>();
			
			public TestContext(Dictionary<string, TestObjectExpression> letExpressions)
			{
				m_letExpressions = letExpressions;
			}
				
		    public override bool TryGetMember(GetMemberBinder binder, out object result)
		    {
				object expressionValue;
				bool foundExpressionValue = m_evaluatedExpressions.TryGetValue(binder.Name, out expressionValue);
				if (foundExpressionValue)
				{
					result = expressionValue;
					return foundExpressionValue;
				}
				
				TestObjectExpression expression;
				bool foundExpression;
		        foundExpression = m_letExpressions.TryGetValue(binder.Name, out expression);
				
				if (foundExpression)
				{
					result = expression();
					m_evaluatedExpressions.Add(binder.Name, result);
				}
				else
					result = null;

				// TODO: Test for false case
				return foundExpression;
		    }
		}

		string m_status = "Not run";
		uint m_run;
		uint m_failures;
		List<Test> m_tests = new List<Test>();
		List<ContextualTest> m_contextualTests = new List<ContextualTest>();
		Dictionary<string, TestObjectExpression> m_letExpressions = new Dictionary<string, TestObjectExpression>();
		
		public void Let(string objectName, TestObjectExpression testObjectExpression)
		{
			m_letExpressions.Add(objectName, testObjectExpression);
		}

		public void Add(Test test)
		{
			m_run++;
			// this needs to move really, need a test to force it
			m_tests.Add(test);
		}

		public void Add(ContextualTest test)
		{
			m_run++;
			m_contextualTests.Add(test);
		}
		
		public void Run ()
		{
			foreach (var test in m_tests) {
				try {
					test();
				} catch (Exception e) {
					m_failures++;
					// TODO: Turn this debugging info into a feature
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
			
			foreach (var test in m_contextualTests) {
				var context = new TestContext(m_letExpressions);
				
				try {
					test(context);
				} catch (Exception e) {
					m_failures++;
					// TODO: Turn this debugging info into a feature
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
			
			m_status = String.Format ("{0} run, {1} failures", m_run, m_failures);
		}

		public string Status()
		{
			return m_status;
		}
	}
}

