using System;
using System.Collections.Generic;

namespace CSharpTestFramework
{
	class MainClass
	{	
		public static void Main (string[] args)
		{
			try {
				var exampleGroupWithFailure = new ExampleGroup();
				exampleGroupWithFailure.Add("A deliberate failing example to prove we can count failing tests",
					() => { throw new Exception(); }
				);
				exampleGroupWithFailure.Run();
				if(exampleGroupWithFailure.Status != "1 run, 1 failures")
					throw new Exception();
			} catch {
				Console.WriteLine("The bootstrap test failed");
				throw;
			}
			
			var mainExampleGroup = new ExampleGroup("Main ExampleGroup");

			ContextFreeExample validExample = () => { };
			ContextFreeExample failingExample = () => { throw new Exception(); };
			
			// TODO: Do we need a bootstrap test for Let?
			mainExampleGroup.Let("exampleGroup", () => new ExampleGroup("ExampleGroup name"));

			mainExampleGroup.Add("An unrun ExampleGroup Status indicates it has not been run",
				(dynamic our) => {
					Expect.That(our.exampleGroup.Status, Is.EqualTo("Not run"));
				}
			);
			
			mainExampleGroup.Add("ExampleGroup Status with no Examples",
				(dynamic our) => {
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.Status, Is.EqualTo("0 run, 0 failures"));
				}
			);
			
			mainExampleGroup.Add("ExampleGroup Status with one valid Example",
				(dynamic our) => {
					our.exampleGroup.Add("Valid Example", validExample);
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.Status, Is.EqualTo("1 run, 0 failures"));
				}
			);
			
			mainExampleGroup.Add("ExampleGroup Status with one valid Example and one failing Example",
				(dynamic our) => {
					our.exampleGroup.Add("Valid Example", validExample);
					our.exampleGroup.Add("Invalid Example", failingExample);
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.Status, Is.EqualTo("2 run, 1 failures"));
				}
			);
			
			mainExampleGroup.Add("ExampleGroup ErrorLog includes exception class and message", (dynamic our) => {
				our.exampleGroup.Add("Invalid Example", (ContextFreeExample)(() => { throw new ApplicationException("Example failure"); }));
				our.exampleGroup.Run();
				// This is a weak spec, but the formatting is WIP
				Expect.That(our.exampleGroup.ErrorLog.Contains("Invalid Example"));
				Expect.That(our.exampleGroup.ErrorLog.Contains("System.ApplicationException"));
				Expect.That(our.exampleGroup.ErrorLog.Contains("Example failure"));
			});
			
			mainExampleGroup.Add("Expect.That ... Is.EqualTo formats an error", (dynamic our) => {
				our.exampleGroup.Add("Invalid Example", (ContextFreeExample)(() => {
					Expect.That("foo", Is.EqualTo("bar"));
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.ErrorLog.Contains("Invalid Example"));
				Expect.That(our.exampleGroup.ErrorLog.Contains("CSharpTestFramework.ExpectationException"));
				Expect.That(our.exampleGroup.ErrorLog.Contains("Expected System.String \"foo\" to be equal to System.String \"bar\", but it was not"));
			});
			
			mainExampleGroup.Add("Expect.That ... Contains.Value formats an error", (dynamic our) => {
				our.exampleGroup.Add("Invalid Example", (ContextFreeExample)(() => {
					try {
						Expect.That("foo baz", Contains.Value("bar"));
					} catch(Exception e) {
						throw e;
					}
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.ErrorLog.Contains("Invalid Example"));
				Expect.That(our.exampleGroup.ErrorLog.Contains("CSharpTestFramework.ExpectationException"));
				Expect.That(our.exampleGroup.ErrorLog.Contains("Expected System.String \"foo baz\" to contain System.String \"bar\", but it did not"));
			});

			mainExampleGroup.Add("ExampleGroup ErrorLog",
				(dynamic our) => {
					our.exampleGroup.Add("Failing Example", (ContextFreeExample)(() => {
						throw new Exception("This fails");
					}));
					our.exampleGroup.Add("Another failing example", (ContextFreeExample)(() => {
						throw new Exception("And this fails too");
					}));
					our.exampleGroup.Run();
					Expect.That(our.exampleGroup.ErrorLog.Contains("This fails"));
				    Expect.That(our.exampleGroup.ErrorLog.Contains("And this fails too"));
				}
			);
			
			mainExampleGroup.Add("ExampleGroup Report includes the name", (dynamic our) => {
				our.exampleGroup.Run();
			    Expect.That(our.exampleGroup.Report.Contains("Example group: ExampleGroup name"));
			});
			
			mainExampleGroup.Add("ExampleGroup Report includes the Example names", (dynamic our) => {
				our.exampleGroup.Add("Example name 1", validExample);
				our.exampleGroup.Add("Example name 2", validExample);
				our.exampleGroup.Run();
			    Expect.That(our.exampleGroup.Report.Contains("- Example name 1"));
				Expect.That(our.exampleGroup.Report.Contains("- Example name 2"));
			});

			mainExampleGroup.Add("ExampleGroup Report flags failing Examples", (dynamic our) => {
				our.exampleGroup.Add("Example name 1", failingExample);
				our.exampleGroup.Add("Example name 2", failingExample);
				our.exampleGroup.Run();
			    Expect.That(our.exampleGroup.Report.Contains("X Example name 1"));
				Expect.That(our.exampleGroup.Report.Contains("X Example name 2"));
			});

			mainExampleGroup.Add("ExampleGroup with Let expression and one passing Example", (dynamic our) => {
				our.exampleGroup.Let("TestObject", (Be)(() => "value of TestObject"));
				our.exampleGroup.Add("Expect the right value", (Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == "value of TestObject");
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status, Is.EqualTo("1 run, 0 failures"));
			});
			
			// TODO: Make the Expect fail correctly and improve the framework to prevent this type of error
			mainExampleGroup.Add("ExampleGroup with Let expression and one failing Example", (dynamic our) => {
				our.exampleGroup.Let("TestObject", (Be)(() => "value of TestObject"));
				our.exampleGroup.Add("Expect the wrong value", (Example)((dynamic ourInner) => {
					// This is actually failing because we used `our` not `ourInner`
					Expect.That(our.TestObject == "wrong value of TestObject");
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status, Is.EqualTo("1 run, 1 failures"));
			});
			
			mainExampleGroup.Add("ExampleGroup with multiple Let expressions", (dynamic our) => {
				our.exampleGroup.Let("TestObject1", (Be)(() => 1));
				our.exampleGroup.Let("TestObject2", (Be)(() => 2));
				our.exampleGroup.Add("Access both Let values", (Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject1 == 1);
					Expect.That(ourInner.TestObject2 == 2);
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status, Is.EqualTo("1 run, 0 failures"));
			});
			
			mainExampleGroup.Add("ExampleGroup only evaluates Let expressions once per Example", (dynamic our) => {
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				our.exampleGroup.Let("TestObject", (Be)(() => accesses["TestObject lookups"] += 1));
				our.exampleGroup.Add("Access Let value twice", (Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 1);
					Expect.That(ourInner.TestObject == 1);
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status, Is.EqualTo("1 run, 0 failures"));
			});
			
			mainExampleGroup.Add("ExampleGroup re-evaluates Let expressions for each Example", (dynamic our) => {
				var accesses = new Dictionary<string, int>() { { "TestObject lookups", 0 } };
				
				our.exampleGroup.Let("TestObject", (Be)(() => accesses["TestObject lookups"] += 1));
				our.exampleGroup.Add("Access first value of Let expression", (Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 1);
				}));
				our.exampleGroup.Add("Access second value of Let expression", (Example)((dynamic ourInner) => {
					Expect.That(ourInner.TestObject == 2);
				}));
				our.exampleGroup.Run();
				Expect.That(our.exampleGroup.Status, Is.EqualTo("2 run, 0 failures"));
			});
			
			mainExampleGroup.Add("ExampleGroups can be nested with Describe", (dynamic our) => {
				our.exampleGroup.Describe("A description of a nested ExampleGroup", (As)((dynamic ourInner) => {
					ourInner.Add("An example", (ContextFreeExample)(() => { }));
					ourInner.Describe("A deeply-nested ExampleGroup name", (As)((dynamic ourInner2) => {
						ourInner2.Add("Another example", (ContextFreeExample)(() => { }));
					}));
				}));
				our.exampleGroup.Run();
				
				Expect.That(our.exampleGroup.Report, Contains.Value("ExampleGroup name"));
				Expect.That(our.exampleGroup.Report, Contains.Value("A description of a nested ExampleGroup"));
				Expect.That(our.exampleGroup.Report, Contains.Value("A deeply-nested ExampleGroup name"));
				Expect.That(our.exampleGroup.Report, Contains.Value("An example"));
				Expect.That(our.exampleGroup.Report, Contains.Value("Another example"));
			});
			
			mainExampleGroup.Add("ExampleGroups with nested Describes count all Examples", (dynamic our) => {
				our.exampleGroup.Describe("A description of a nested ExampleGroup", (As)((dynamic ourInner) => {
					ourInner.Add("An example", (ContextFreeExample)(() => { }));
					ourInner.Describe("A deeply-nested ExampleGroup name", (As)((dynamic ourInner2) => {
						ourInner2.Add("An example", (ContextFreeExample)(() => { throw new Exception("Fail"); }));
					}));
				}));
				our.exampleGroup.Run();
				
				Expect.That(our.exampleGroup.Status, Is.EqualTo("2 run, 1 failures"));
			});
			
			mainExampleGroup.Add("Examples in nested ExampleGroups can use Let expressions from higher up", (dynamic our) => {
				our.exampleGroup.Let("TheNumberOne", (Be)(() => 1));
				our.exampleGroup.Describe("A description of a nested ExampleGroup", (As)((dynamic ourInner) => {
					ourInner.Add("An example", (Example)((dynamic context) => { Expect.That(context.TheNumberOne, Is.EqualTo(1)); }));
					ourInner.Describe("A deeply-nested ExampleGroup name", (As)((dynamic ourInner2) => {
						ourInner2.Add("An example", (Example)((dynamic context2) => { Expect.That(context2.TheNumberOne, Is.EqualTo(1)); }));
					}));
				}));
				our.exampleGroup.Run();
				
				Expect.That(our.exampleGroup.Status, Is.EqualTo("2 run, 0 failures"));
			});
			
			// TODO: ErrorLog from nested Examples is included in nested log
			
			// Expectations
			mainExampleGroup.Add("Expect.That ...", () => {
				Expect.That(true);
			});
			
			mainExampleGroup.Add("Expect.That ... Contains.Value", () => {
				Expect.That("foo bar baz", Contains.Value("bar"));
			});
			
			mainExampleGroup.Run();
			
			Console.WriteLine(mainExampleGroup.Report);
			Console.WriteLine(mainExampleGroup.ErrorLog);
			Console.WriteLine(mainExampleGroup.Status);
		}
	}
}

