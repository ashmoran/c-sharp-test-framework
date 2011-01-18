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
				var exampleGroupWithFailure = new ExampleGroup();
				exampleGroupWithFailure.Add(
					() => { throw new Exception(); }
				);
				exampleGroupWithFailure.Run();
				if(exampleGroupWithFailure.Status() != "1 run, 1 failures")
					throw new Exception();
			} catch(Exception e) {
				Console.WriteLine("The bootstrap test failed");
				throw e;
			}
			
			var mainExampleGroup = new ExampleGroup();

			ContextFreeExample passingTest = () => { };
			ContextFreeExample failingTest = () => { throw new Exception(); };
			
			// TODO: Do we need a bootstrap test for Let?
			mainExampleGroup.Let("exampleGroup", () => new ExampleGroup());

			// An unrun ExampleGroup
			mainExampleGroup.Add(
				(dynamic our) => {
					Expect.That(our.exampleGroup.Status() == "Not run");
				}
			);
			
			// An empty ExampleGroup
			mainExampleGroup.Add(
				(dynamic our) => {
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.Status() == "0 run, 0 failures");
				}
			);
			
			// A group with one passing test
			mainExampleGroup.Add(
				(dynamic our) => {
					our.exampleGroup.Add(passingTest);
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.Status() == "1 run, 0 failures");
				}
			);
			
			// A group with one passing test and one failing test
			mainExampleGroup.Add(
				(dynamic our) => {
					our.exampleGroup.Add(passingTest);
					our.exampleGroup.Add(failingTest);
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.Status() == "2 run, 1 failures");
				}
			);
			
			// Error output from a failing test
			mainExampleGroup.Add(
				(dynamic our) => {
					our.exampleGroup.Add((ContextFreeExample)(() => { throw new Exception("This fails"); }));
					our.exampleGroup.Add((ContextFreeExample)(() => { throw new Exception("And this fails too"); }));
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.ErrorLog.Contains("This fails"));
				    Expect.That(our.exampleGroup.ErrorLog.Contains("And this fails too"));
				}
			);

			// Let block with passing example
			mainExampleGroup.Add((dynamic our) => {
				our.exampleGroup.Let("TestObject", (Be)(() => "value of TestObject"));
				our.exampleGroup.Add((Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == "value of TestObject");
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status() == "1 run, 0 failures");
			});
			
			// Let block with failing example
			mainExampleGroup.Add((dynamic our) => {
				our.exampleGroup.Let("TestObject", (Be)(() => "value of TestObject"));
				our.exampleGroup.Add((Example)((dynamic ourInner) => {
					// TODO make this fail correctly and improve the framework to prevent this type of error
					Expect.That(our.TestObject == "wrong value of TestObject");
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status() == "1 run, 1 failures");
			});
			
			// Multiple let blocks
			mainExampleGroup.Add((dynamic our) => {
				our.exampleGroup.Let("TestObject1", (Be)(() => 1));
				our.exampleGroup.Let("TestObject2", (Be)(() => 2));
				our.exampleGroup.Add((Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject1 == 1);
					Expect.That(ourInner.TestObject2 == 2);
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status() == "1 run, 0 failures");
			});
			
			// Let expressions are evaluated once within a test
			mainExampleGroup.Add((dynamic our) => {
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				our.exampleGroup.Let("TestObject", (Be)(() => accesses["TestObject lookups"] += 1));
				our.exampleGroup.Add((Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 1);
					Expect.That(ourInner.TestObject == 1);
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status() == "1 run, 0 failures");
			});
			
			// Let expressions are re-evaluated for each test
			mainExampleGroup.Add((dynamic our) => {
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				our.exampleGroup.Let("TestObject", (Be)(() => accesses["TestObject lookups"] += 1));
				our.exampleGroup.Add((Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 1);
				}));
				our.exampleGroup.Add((Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 2);
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status() == "2 run, 0 failures");
			});
			
			mainExampleGroup.Run();
			
			Console.WriteLine(mainExampleGroup.ErrorLog);
			Console.WriteLine(mainExampleGroup.Status());
		}
	}
}

