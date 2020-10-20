using System;
using System.IO;
using System.Runtime.Serialization.Json;

public class Serialisation
{
    public static byte[] SerialiseData<T>(T data)
    {
        using (var ms = new MemoryStream())
        {
            var serialiser = new DataContractJsonSerializer(typeof(T));
            serialiser.WriteObject(ms, data);
            return ms.ToArray();
        }
    }

    public static T DeserialiseData<T>(byte[] data)
    {
        try
        {
            using (var ms = new MemoryStream(data))
            {
                var serialiser = new DataContractJsonSerializer(typeof(T));
                return (T)serialiser.ReadObject(ms);
            }
        }
        catch (Exception e)
        {
            Console.Error.Write(e.StackTrace);
            return default(T);
        }
    }
}