using System.Text;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace Infrastructure.Services;

public class UrlSafeEncoder : IUrlSafeEncoder
{
    public string EncodeString(string plainText)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public string DecodeString(string encodedText)
    {
        byte[] bytes = WebEncoders.Base64UrlDecode(encodedText);
        return Encoding.UTF8.GetString(bytes);
    }
}