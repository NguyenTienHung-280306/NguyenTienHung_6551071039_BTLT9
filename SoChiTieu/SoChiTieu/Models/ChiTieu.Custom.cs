using System.Globalization;

namespace SoChiTieu.Models;

public partial class ChiTieu
{
    public string MaHienThi => MaCt.ToString("D3");

    public string NgayChiHienThi => NgayChi.ToString("dd/MM/yyyy");

    public string SoTienHienThi => SoTien.ToString("#,##0", CultureInfo.GetCultureInfo("vi-VN"));
}
