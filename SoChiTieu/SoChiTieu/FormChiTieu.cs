using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SoChiTieu.Models;

namespace SoChiTieu
{
    public partial class FormChiTieu : Form
    {
        private int? _selectedMaCT = null;
        private bool _isLoadingData = false;

        private readonly string[] _loaiChiList = new[]
        {
            "Ăn uống",
            "Đi lại",
            "Di chuyển",
            "Học tập",
            "Giải trí",
            "Cá nhân",
            "Hóa đơn",
            "Khác"
        };

        public FormChiTieu()
        {
            InitializeComponent();
        }

        private void FormChiTieu_Load(object sender, EventArgs e)
        {
            InitComboBoxes();
            ResetSearchFilter();
            LoadData();
        }

        private void InitComboBoxes()
        {
            // Danh sách loại chi nhập liệu
            cboLoaiChi.Items.Clear();
            cboLoaiChi.Items.AddRange(_loaiChiList);
            if (cboLoaiChi.Items.Count > 0)
            {
                cboLoaiChi.SelectedIndex = 0;
            }

            // Danh sách loại chi tìm kiếm
            cboLocLoaiChi.Items.Clear();
            cboLocLoaiChi.Items.Add("Tất cả");
            cboLocLoaiChi.Items.AddRange(_loaiChiList);
            cboLocLoaiChi.SelectedIndex = 0;
        }

        private void ResetSearchFilter()
        {
            txtTimNoiDung.Clear();
            cboLocLoaiChi.SelectedIndex = 0;

            var today = DateTime.Today;
            dtpTuNgay.Value = new DateTime(today.Year, today.Month, 1);
            dtpDenNgay.Value = today;
        }

        private void LamMoiThongTin()
        {
            _selectedMaCT = null;
            dtpNgayChi.Value = DateTime.Today;
            txtNoiDung.Clear();
            if (cboLoaiChi.Items.Count > 0)
            {
                cboLoaiChi.SelectedIndex = 0;
            }
            nudSoTien.Value = 0;
            txtGhiChu.Clear();
            dgvChiTieu.ClearSelection();
        }

        private void LoadData()
        {
            try
            {
                _isLoadingData = true;

                using var context = new QL_ChiTieuContext();
                var list = context.ChiTieus
                    .AsNoTracking()
                    .OrderBy(c => c.MaCt)
                    .ToList();

                dgvChiTieu.AutoGenerateColumns = false;
                dgvChiTieu.DataSource = list;

                CapNhatTongChi(list);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi truy cập cơ sở dữ liệu khi tải danh sách chi tiêu: {ErrorMessage}", ex.Message);
                MessageBox.Show("Đã xảy ra lỗi khi kết nối hoặc tải dữ liệu từ CSDL. Vui lòng kiểm tra lại cấu hình kết nối!", 
                    "Lỗi kết nối CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoadingData = false;
            }
        }

        private void CapNhatTongChi(IEnumerable<ChiTieu> list)
        {
            var tong = list.Sum(x => x.SoTien);
            lblTongChi.Text = $"Tổng chi: {tong.ToString("#,##0", CultureInfo.GetCultureInfo("vi-VN"))} đ";
        }

        private bool KiemTraHopLe()
        {
            if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Nội dung không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNoiDung.Focus();
                return false;
            }

            if (nudSoTien.Value <= 0)
            {
                MessageBox.Show("Số tiền phải lớn hơn 0!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudSoTien.Focus();
                return false;
            }

            if (dtpNgayChi.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Ngày chi không được lớn hơn ngày hiện tại!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpNgayChi.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(cboLoaiChi.Text))
            {
                MessageBox.Show("Vui lòng chọn loại chi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLoaiChi.Focus();
                return false;
            }

            return true;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (!KiemTraHopLe())
            {
                return;
            }

            try
            {
                using var context = new QL_ChiTieuContext();
                var chiTieu = new ChiTieu
                {
                    NgayChi = DateOnly.FromDateTime(dtpNgayChi.Value.Date),
                    NoiDung = txtNoiDung.Text.Trim(),
                    LoaiChi = cboLoaiChi.Text.Trim(),
                    SoTien = nudSoTien.Value,
                    GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null : txtGhiChu.Text.Trim()
                };

                context.ChiTieus.Add(chiTieu);
                context.SaveChanges();

                Log.Information("Thêm khoản chi thành công [MaCT: {MaCT}]: Ngày={NgayChi}, Nội dung='{NoiDung}', Loại chi='{LoaiChi}', Số tiền={SoTien:N0} đ, Ghi chú='{GhiChu}'",
                    chiTieu.MaCt, chiTieu.NgayChi, chiTieu.NoiDung, chiTieu.LoaiChi, chiTieu.SoTien, chiTieu.GhiChu);

                MessageBox.Show("Thêm khoản chi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LamMoiThongTin();
                LoadData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi truy cập cơ sở dữ liệu khi thêm khoản chi: {ErrorMessage}", ex.Message);
                MessageBox.Show("Đã xảy ra lỗi khi thêm khoản chi vào cơ sở dữ liệu. Vui lòng thử lại!",
                    "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (_selectedMaCT == null)
            {
                MessageBox.Show("Vui lòng chọn một dòng khoản chi trong danh sách để sửa!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!KiemTraHopLe())
            {
                return;
            }

            try
            {
                using var context = new QL_ChiTieuContext();
                var chiTieu = context.ChiTieus.Find(_selectedMaCT.Value);

                if (chiTieu == null)
                {
                    MessageBox.Show("Khoản chi được chọn không tồn tại hoặc đã bị xóa!", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LamMoiThongTin();
                    LoadData();
                    return;
                }

                chiTieu.NgayChi = DateOnly.FromDateTime(dtpNgayChi.Value.Date);
                chiTieu.NoiDung = txtNoiDung.Text.Trim();
                chiTieu.LoaiChi = cboLoaiChi.Text.Trim();
                chiTieu.SoTien = nudSoTien.Value;
                chiTieu.GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null : txtGhiChu.Text.Trim();

                context.SaveChanges();

                Log.Information("Sửa khoản chi thành công [MaCT: {MaCT}]: Ngày={NgayChi}, Nội dung='{NoiDung}', Loại chi='{LoaiChi}', Số tiền={SoTien:N0} đ, Ghi chú='{GhiChu}'",
                    chiTieu.MaCt, chiTieu.NgayChi, chiTieu.NoiDung, chiTieu.LoaiChi, chiTieu.SoTien, chiTieu.GhiChu);

                MessageBox.Show("Cập nhật khoản chi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi truy cập cơ sở dữ liệu khi sửa khoản chi MaCT={MaCT}: {ErrorMessage}", _selectedMaCT, ex.Message);
                MessageBox.Show("Đã xảy ra lỗi khi sửa khoản chi trong cơ sở dữ liệu. Vui lòng thử lại!",
                    "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (_selectedMaCT == null)
            {
                MessageBox.Show("Vui lòng chọn một dòng khoản chi trong danh sách để xóa!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialogResult = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa khoản chi '{txtNoiDung.Text}' (Mã: {_selectedMaCT.Value:D3}) không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using var context = new QL_ChiTieuContext();
                var chiTieu = context.ChiTieus.Find(_selectedMaCT.Value);

                if (chiTieu != null)
                {
                    var maCt = chiTieu.MaCt;
                    var noiDung = chiTieu.NoiDung;

                    context.ChiTieus.Remove(chiTieu);
                    context.SaveChanges();

                    Log.Information("Xóa khoản chi thành công [MaCT: {MaCT}]: Nội dung='{NoiDung}'", maCt, noiDung);

                    MessageBox.Show("Đã xóa khoản chi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LamMoiThongTin();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Khoản chi đã không còn tồn tại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LamMoiThongTin();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi truy cập cơ sở dữ liệu khi xóa khoản chi MaCT={MaCT}: {ErrorMessage}", _selectedMaCT, ex.Message);
                MessageBox.Show("Đã xảy ra lỗi khi xóa khoản chi khỏi cơ sở dữ liệu. Vui lòng thử lại!",
                    "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiThongTin();
            ResetSearchFilter();
            LoadData();
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            ThucHienTimKiem();
        }

        private void ThucHienTimKiem()
        {
            var tuNgay = DateOnly.FromDateTime(dtpTuNgay.Value.Date);
            var denNgay = DateOnly.FromDateTime(dtpDenNgay.Value.Date);

            if (tuNgay > denNgay)
            {
                MessageBox.Show("Từ ngày không được lớn hơn Đến ngày!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpTuNgay.Focus();
                return;
            }

            try
            {
                _isLoadingData = true;

                using var context = new QL_ChiTieuContext();
                var query = context.ChiTieus.AsNoTracking().AsQueryable();

                // Lọc theo khoảng ngày
                query = query.Where(c => c.NgayChi >= tuNgay && c.NgayChi <= denNgay);

                // Lọc theo loại chi
                var loaiChiChon = cboLocLoaiChi.SelectedItem?.ToString();
                if (!string.IsNullOrWhiteSpace(loaiChiChon) && loaiChiChon != "Tất cả")
                {
                    query = query.Where(c => c.LoaiChi == loaiChiChon);
                }

                // Lọc theo nội dung
                var tuKhoa = txtTimNoiDung.Text.Trim();
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(c => c.NoiDung.Contains(tuKhoa) || (c.GhiChu != null && c.GhiChu.Contains(tuKhoa)));
                }

                var result = query.OrderBy(c => c.MaCt).ToList();

                dgvChiTieu.DataSource = result;
                CapNhatTongChi(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi truy cập cơ sở dữ liệu khi tìm kiếm/lọc: {ErrorMessage}", ex.Message);
                MessageBox.Show("Đã xảy ra lỗi khi tìm kiếm trong cơ sở dữ liệu. Vui lòng thử lại!", 
                    "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoadingData = false;
            }
        }

        private void dgvChiTieu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                HienThiThongTinDongDuocChon(e.RowIndex);
            }
        }

        private void dgvChiTieu_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoadingData)
            {
                return;
            }

            if (dgvChiTieu.CurrentRow != null && dgvChiTieu.CurrentRow.Index >= 0)
            {
                HienThiThongTinDongDuocChon(dgvChiTieu.CurrentRow.Index);
            }
        }

        private void HienThiThongTinDongDuocChon(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvChiTieu.Rows.Count)
            {
                return;
            }

            if (dgvChiTieu.Rows[rowIndex].DataBoundItem is ChiTieu selected)
            {
                _selectedMaCT = selected.MaCt;
                dtpNgayChi.Value = new DateTime(selected.NgayChi.Year, selected.NgayChi.Month, selected.NgayChi.Day);
                txtNoiDung.Text = selected.NoiDung;

                if (cboLoaiChi.Items.Contains(selected.LoaiChi))
                {
                    cboLoaiChi.SelectedItem = selected.LoaiChi;
                }
                else
                {
                    cboLoaiChi.Text = selected.LoaiChi;
                }

                nudSoTien.Value = selected.SoTien;
                txtGhiChu.Text = selected.GhiChu ?? string.Empty;
            }
        }
    }
}
