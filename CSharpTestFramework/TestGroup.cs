using System;
using System.Collections.Generic;

namespace CSharpTestFramework
{
	public delegate void Test();
	
	public class TestGroup
	{
		private string m_status = "Not run";
		private uint m_run;
		private uint m_failures;
		private List<Test> m_tests = new List<Test>();
		
		public void Add(Test test)
		{
			m_run++; // this needs to move really, need a test to force it
			m_tests.Add(test);
		}
		
		public void Run()
		{
			foreach(var test in m_tests)
			{
				try {
					test();
				} catch {
					m_failures++;
				}
			}
			m_status = String.Format("{0} run, {1} failures", m_run, m_failures);
		}
		
		public string Status()
		{
			return m_status;
		}
	}
}

