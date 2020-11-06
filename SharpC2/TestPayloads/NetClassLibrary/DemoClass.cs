namespace TestLibrary
{
    public class TestClass
    {
        public static string TestMethod1()
        {
            return "Hello from Test Method 1";
        }

        public static string TestMethod2(string message)
        {
            return $"Hello from Test Method 2.  You said {message}.";
        }
    }
}