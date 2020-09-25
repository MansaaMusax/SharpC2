public class CommStateObject
{
    public object Worker { get; set; } = null;
    public object Handler { get; set; } = null;
    public byte[] Buffer { get; set; } = new byte[65535];
    public byte[] SwapBuffer { get; set; } = null;
}