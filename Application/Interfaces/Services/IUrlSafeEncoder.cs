namespace Application.Interfaces.Services;

public interface IUrlSafeEncoder
{
    string EncodeString(string plainText);
    string DecodeString(string encodedText);
}