using System;

namespace CryptoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var alice = new Alice();
            var Stage0Request = alice.Stage0Request();

            var bob = new Bob();
            var Stage0Response = bob.Stage0Response(Stage0Request);

            var Stage1Request = alice.Stage1Request(Stage0Response);
            var Stage1Response = bob.Stage1Response(Stage1Request);

            var Stage2Request = alice.Stage2Request(Stage1Response, out byte[] iv);
            bob.Stage2Response(Stage2Request, iv);
        }
    }
}