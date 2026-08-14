using System;
using System.Reflection;

namespace FunctionsMonolith.Tests
{
    public static class Program
    {
        public static int Main()
        {
            int failures = 0;
            failures += Run("ResourceEntryTest", ResourceEntryTest.Run);
            failures += Run("PartitionRouterTest", PartitionRouterTest.Run);
            failures += Run("ReplicationManagerTest", ReplicationManagerTest.Run);
            failures += Run("AppManagerTest", AppManagerTest.Run);
            failures += Run("AppServiceTest", AppServiceTest.Run);
            failures += Run("GetFunctionTest", GetFunctionTest.Run);
            failures += Run("PutFunctionTest", PutFunctionTest.Run);
            failures += Run("DeleteFunctionTest", DeleteFunctionTest.Run);
            failures += Run("UiNavigationTest", UiNavigationTest.Run);
            failures += Run("CsharpBuiltinUsageTest", CsharpBuiltinUsageTest.Run);

            Console.WriteLine(failures == 0 ? "ALL TESTS PASS" : "TESTS FAILED: " + failures);
            return failures == 0 ? 0 : 1;
        }

        private static int Run(string name, Func<int> test)
        {
            try
            {
                int failed = test();
                Console.WriteLine(name + ": " + (failed == 0 ? "PASS" : "FAIL (" + failed + ")"));
                return failed;
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine(name + ": FAIL " + ex.InnerException);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(name + ": FAIL " + ex);
                return 1;
            }
        }
    }
}
