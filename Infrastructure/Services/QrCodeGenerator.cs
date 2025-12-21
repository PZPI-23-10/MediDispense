using Application.Interfaces.Services;
using QRCoder;

namespace Infrastructure.Services;

public class QrCodeGenerator : IQrCodeGenerator
{
    public byte[] GenerateQrCode(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }
}