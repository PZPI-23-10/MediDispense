namespace Application.Interfaces.Services;

public interface IQrCodeGenerator
{
    byte[] GenerateQrCode(string payload);
}