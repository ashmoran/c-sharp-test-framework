using System;
using System.Collections.Generic;
using System.Dynamic;

namespace CSharpTestFramework
{
	public delegate void Example(dynamic context);
	public delegate void ContextFreeExample();
	public delegate object Be(); // As in: Let("Foo", (Be)(() => "Bar))

	public class TestGroup
	{
		// TODO: Extract and test independently
		public class ExampleContext : DynamicObject
		{
			// TODO: Name this type
			Dictionary<string, Be> m_letExpressions;
			
			Dictionary<string, object> m_evaluatedExpressions = new Dictionary<string, object>();
			
			public ExampleContext(Dictionary<string, Be> letExpressions)
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
				
				Be expression;
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
		List<Example> m_examples = new List<Example>();
		Dictionary<string, Be> m_letExpressions = new Dictionary<string, Be>();
		
		public void Let(string objectName, Be letExpression)
		{
			m_letExpressions.Add(objectName, letExpression);
		}

		public void Add(Example example)
		{
			m_examples.Add(example);
		}
		
		public void Add(ContextFreeExample example)
		{
			m_examples.Add((dynamic unusedContext) => example());
		}

		public void Run ()
		{
			foreach (var example in m_examples) {
				var context = new ExampleContext(m_letExpressions);

				m_run++;
				
				try {
					example(context);
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

