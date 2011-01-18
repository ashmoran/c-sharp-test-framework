using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace CSharpTestFramework
{
	public delegate void Example(dynamic context);
	// TODO: Rename to SimpleExample?
	public delegate void ContextFreeExample();
	public delegate object Be(); // As in: Let("Foo", (Be)(() => "Bar))

	public class ExampleGroup
	{
		public class LetExpressionDictionary : Dictionary<string, Be> { };
		
		// TODO: Extract and test independently
		public class ExampleContext : DynamicObject
		{
			// TODO: Name this type
			LetExpressionDictionary m_letExpressions;
			
			Dictionary<string, object> m_evaluatedExpressions = new Dictionary<string, object>();
			
			public ExampleContext(LetExpressionDictionary letExpressions)
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
		
		string m_name;
		string m_status = "Not run";
		string m_report = "";
		string m_errorLog = "";
		uint m_run;
		uint m_failures;
		List<NamedExample> m_examples = new List<NamedExample>();
		LetExpressionDictionary m_letExpressions = new LetExpressionDictionary();
		
		public ExampleGroup(string name = "")
		{
			m_name = name;
		}
		
		public string ErrorLog
		{
			get
			{
				return m_errorLog;
			}
		}
		
		public string Report
		{
			get
			{
				return m_report;
			}
		}
		
		public void Let(string objectName, Be letExpression)
		{
			m_letExpressions.Add(objectName, letExpression);
		}
		
		// TODO: Extract and test independently
		class NamedExample
		{
			string m_name;
			Example m_example;
			
			public NamedExample(string name, Example example)
			{
				m_name = name;
				m_example = example;
			}
			
			public string Name
			{
				get
				{
					return m_name;	
				}
			}
			
			public void Run(ExampleContext exampleContext)
			{
				m_example(exampleContext);
			}
		}

		public void Add(string name, Example example)
		{
			m_examples.Add(new NamedExample(name, example));
		}
		
		public void Add(string name, ContextFreeExample example)
		{
			m_examples.Add(new NamedExample(name, (dynamic unusedContext) => example()));
		}
		
		// TODO: Extract reporting
		public void Run()
		{
			m_report += "Example group: " + m_name + "\n";
			
			foreach (var example in m_examples) {
				var context = new ExampleContext(m_letExpressions);

				m_run++;
				
				try {
					example.Run(context);
					m_report += "-";
				} catch (Exception exception) {
					m_failures++;
					m_errorLog += example.Name + " >> " + exception.GetType() + ": " + exception.Message + "\n" + exception.StackTrace + "\n";
					m_report += "X";
				}
				
				m_report += " " + example.Name + "\n";
			}
			
			m_status = String.Format ("{0} run, {1} failures", m_run, m_failures);
		}
		
		// TODO: Make this into a property
		public string Status()
		{
			return m_status;
		}
	}
}

