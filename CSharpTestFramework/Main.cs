using System;
using System.Collections.Generic;

namespace CSharpTestFramework
{
	class MainClass
	{	
		public static void Main (string[] args)
		{
			// We need this one example to prove that the test framework can actually 
			// catch failures - without it, nothing else can be relied on
			try {
				var testGroupWithFailure = new TestGroup();
				testGroupWithFailure.Add(
					() => { throw new Exception(); }
				);
				testGroupWithFailure.Run();
				if(testGroupWithFailure.Status() != "1 run, 1 failures")
					throw new Exception();
			} catch(Exception e) {
				Console.WriteLine("The bootstrap test failed");
				throw e;
			}
			
			var mainTestGroup = new TestGroup();

			Test passingTest = () => { };
			Test failingTest = () => { throw new Exception(); };
			
			// TODO: Do we need a bootstrap test for Let?
			mainTestGroup.Let("testGroup", () => new TestGroup());

			// An unrun TestGroup
			mainTestGroup.Add(
				(dynamic our) => {
					Expect.That(our.testGroup.Status() == "Not run");
				}
			);
			
			// An empty TestGroup
			mainTestGroup.Add(
				(dynamic our) => {
					our.testGroup.Run();
					Expect.That(our.testGroup.Status() == "0 run, 0 failures");
				}
			);
			
			// A group with one passing test
			mainTestGroup.Add(
				(dynamic our) => {
					our.testGroup.Add(passingTest);
					our.testGroup.Run();
					Expect.That(our.testGroup.Status() == "1 run, 0 failures");
				}
			);
			
			// A group with one passing test and one failing test
			mainTestGroup.Add(
				(dynamic our) => {
					our.testGroup.Add(passingTest);
					our.testGroup.Add(failingTest);
					our.testGroup.Run();
					Expect.That(our.testGroup.Status() == "2 run, 1 failures");
				}
			);

			// Let block with passing example
			mainTestGroup.Add((dynamic our) => {
				our.testGroup.Let("TestObject", (TestObjectExpression)(() => "value of TestObject"));
				our.testGroup.Add((ContextualTest)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == "value of TestObject");
				}));
				our.testGroup.Run();
				Expect.That(our.testGroup.Status() == "1 run, 0 failures");
			});
			
			// Let block with failing example
			mainTestGroup.Add((dynamic our) => {
				our.testGroup.Let("TestObject", (TestObjectExpression)(() => "value of TestObject"));
				our.testGroup.Add((ContextualTest)((dynamic ourInner) => {
					Expect.That(our.TestObject == "wrong value of TestObject");
				}));
				our.testGroup.Run();
				Expect.That(our.testGroup.Status() == "1 run, 1 failures");
			});
			
			// Multiple let blocks
			mainTestGroup.Add((dynamic our) => {
				our.testGroup.Let("TestObject1", (TestObjectExpression)(() => 1));
				our.testGroup.Let("TestObject2", (TestObjectExpression)(() => 2));
				our.testGroup.Add((ContextualTest)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject1 == 1);
					Expect.That(ourInner.TestObject2 == 2);
				}));
				our.testGroup.Run();
				Expect.That(our.testGroup.Status() == "1 run, 0 failures");
			});
			
			// Let expressions are evaluated once within a test
			mainTestGroup.Add((dynamic our) => {
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				our.testGroup.Let("TestObject", (TestObjectExpression)(() => accesses["TestObject lookups"] += 1));
				our.testGroup.Add((ContextualTest)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 1);
					Expect.That(ourInner.TestObject == 1);
				}));
				our.testGroup.Run();
				Expect.That(our.testGroup.Status() == "1 run, 0 failures");
			});
			
			// Let expressions are re-evaluated for each test
			mainTestGroup.Add((dynamic our) => {
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				our.testGroup.Let("TestObject", (TestObjectExpression)(() => accesses["TestObject lookups"] += 1));
				our.testGroup.Add((ContextualTest)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 1);
				}));
				our.testGroup.Add((ContextualTest)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 2);
				}));
				our.testGroup.Run();
				Expect.That(our.testGroup.Status() == "2 run, 0 failures");
			});
			
			mainTestGroup.Run();
			
			Console.WriteLine(mainTestGroup.Status());
		}
	}
}

