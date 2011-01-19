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
	public delegate void As(dynamic exampleGroup); // As in: Describe("Foo", (As)((dynamic group) => { ... }))
	
	public class LetExpressionDictionary : Dictionary<string, Be> {
		public LetExpressionDictionary() : base() { }
		public LetExpressionDictionary(LetExpressionDictionary template) : base(template) { }
	};

	interface SpecComponent
	{
		int ExamplesRun { get; }
 		int ExamplesFailed { get; }
		string Report { get; }
		string ErrorLog { get; }
		void Run(ExampleContext inheritedContext = null);
	}
	
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
	
	// TODO: Extract and test independently
	class NamedExample : SpecComponent
	{
		string m_name;
		Example m_example;
		
		string m_report;
		string m_errorLog;
		int m_failures;
		
		public NamedExample(string name, Example example)
		{
			m_name = name;
			m_example = example;
		}
		
		public string Name { get { return m_name; } }
		public int ExamplesRun { get { return 1; } } // TODO: Describe the situation where we care about this being 0, if it exists
		public int ExamplesFailed { get { return m_failures; } }
		public string Report { get { return m_report; }}
		public string ErrorLog { get { return m_errorLog; } }
					
		public void Run(ExampleContext exampleContext)
		{
			try {
				m_example(exampleContext);
				m_report += "-";
			} catch (Exception exception) {
				m_failures++;
				m_errorLog += m_name + " >> " + exception.GetType() + ": " + exception.Message + "\n" + exception.StackTrace + "\n";
				m_report += "X";
			}
			
			m_report += " " + m_name + "\n";
		}
	}

	public class ExampleGroup : SpecComponent
	{			
		string m_name;
		string m_status = "Not run";
		string m_report = "";
		string m_errorLog = "";
		int m_run;
		int m_failures;
		List<SpecComponent> m_specComponents = new List<SpecComponent>();
		LetExpressionDictionary m_letExpressions = new LetExpressionDictionary();
		
		public ExampleGroup(string name = "", LetExpressionDictionary inheritedLetExpressions = null)
		{
			m_name = name;
			if (inheritedLetExpressions != null)
			{
				m_letExpressions = new LetExpressionDictionary(inheritedLetExpressions);
			}
		}
		
		public string Status { get { return m_status; } }
		public int ExamplesRun { get { return m_run; } }
		public int ExamplesFailed { get { return m_failures; } }
		public string ErrorLog { get { return m_errorLog; } }
		public string Report { get { return m_report; } }
		
		public void Describe(string exampleGroupName, As examples)
		{
			var exampleGroup = new ExampleGroup(exampleGroupName, m_letExpressions);
			examples(exampleGroup);
			m_specComponents.Add(exampleGroup);
		}
		
		public void Let(string objectName, Be letExpression)
		{
			m_letExpressions.Add(objectName, letExpression);
		}

		public void Add(string name, Example example)
		{
			m_specComponents.Add(new NamedExample(name, example));
		}
		
		public void Add(string name, ContextFreeExample example)
		{
			m_specComponents.Add(new NamedExample(name, (dynamic unusedContext) => example()));
		}
		
		// TODO: Extract reporting
		// TODO: Fix this null-defaulted parameter - it's because a top-level ExampleGroup creates the first context
		public void Run(ExampleContext inheritedContext = null)
		{
			m_report += "Example group: " + m_name + "\n";
			
			foreach (var specComponent in m_specComponents)
			{
				var context = new ExampleContext(m_letExpressions);
				
				specComponent.Run(context);
				
				m_run += specComponent.ExamplesRun;
				m_failures += specComponent.ExamplesFailed;
				m_report += specComponent.Report;
				m_errorLog += specComponent.ErrorLog;
			}
			
			m_status = String.Format ("{0} run, {1} failures", m_run, m_failures);
		}		
	}
}

