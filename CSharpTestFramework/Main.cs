using System;

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

			// An unrun TestGroup
			mainTestGroup.Add(
				() => {
					var testGroup = new TestGroup();
					Expect.That(testGroup.Status() == "Not run");
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
			
			mainTestGroup.Run();
			
			Console.WriteLine(mainTestGroup.Status());
		}
	}
}

