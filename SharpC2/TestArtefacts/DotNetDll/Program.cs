using System.Windows.Forms;

public class DotNetDll
{
    public static string Test(string input)
    {
        MessageBox.Show(input);
        return "Hello from .NET DLL";
    }
}