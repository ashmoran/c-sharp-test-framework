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
				() => {
					var testGroup = new TestGroup();
					testGroup.Run();
					Expect.That(testGroup.Status() == "0 run, 0 failures");
				}
			);
			
			// A group with one passing test
			mainTestGroup.Add(
				() => {
					var testGroup = new TestGroup();
					testGroup.Add(passingTest);
					testGroup.Run();
					Expect.That(testGroup.Status() == "1 run, 0 failures");
				}
			);
			
			// A group with one passing test and one failing test
			mainTestGroup.Add(
				() => {
					var testGroup = new TestGroup();
					testGroup.Add(passingTest);
					testGroup.Add(failingTest);
					testGroup.Run();
					Expect.That(testGroup.Status() == "2 run, 1 failures");
				}
			);

			// Let block with passing example
			mainTestGroup.Add(() => {
				var testGroup = new TestGroup();
				testGroup.Let("TestObject", () => "value of TestObject");
				testGroup.Add((dynamic our) => {
					Expect.That(our.TestObject == "value of TestObject");
				});
				testGroup.Run();
				Expect.That(testGroup.Status() == "1 run, 0 failures");
			});
			
			// Let block with failing example
			mainTestGroup.Add(() => {
				var testGroup = new TestGroup();
				testGroup.Let("TestObject", () => "value of TestObject");
				testGroup.Add((dynamic our) => {
					Expect.That(our.TestObject == "wrong value of TestObject");
				});
				testGroup.Run();
				Expect.That(testGroup.Status() == "1 run, 1 failures");
			});
			
			// Multiple let blocks
			mainTestGroup.Add(() => {
				var testGroup = new TestGroup();
				testGroup.Let("TestObject1", () => 1);
				testGroup.Let("TestObject2", () => 2);
				testGroup.Add((dynamic our) => {
					Expect.That(our.TestObject1 == 1);
					Expect.That(our.TestObject2 == 2);
				});
				testGroup.Run();
				Expect.That(testGroup.Status() == "1 run, 0 failures");
			});
			
			// Let expressions are evaluated once within a test
			mainTestGroup.Add(() => {
				var testGroup = new TestGroup();
				
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				testGroup.Let("TestObject", () => accesses["TestObject lookups"] += 1);
				testGroup.Add((dynamic our) => {
					Expect.That(our.TestObject == 1);
					Expect.That(our.TestObject == 1);
				});
				testGroup.Run();
				Expect.That(testGroup.Status() == "1 run, 0 failures");
			});
			
			// Let expressions are re-evaluated for each test
			mainTestGroup.Add(() => {
				var testGroup = new TestGroup();
				
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				testGroup.Let("TestObject", () => accesses["TestObject lookups"] += 1);
				testGroup.Add((dynamic our) => {
					Expect.That(our.TestObject == 1);
				});
				testGroup.Add((dynamic our) => {
					Expect.That(our.TestObject == 2);
				});
				testGroup.Run();
				Console.WriteLine(String.Format("x {0}", accesses["TestObject lookups"]));
				Expect.That(testGroup.Status() == "2 run, 0 failures");
			});
			
			mainTestGroup.Run();
			
			Console.WriteLine(mainTestGroup.Status());
		}
	}
}

