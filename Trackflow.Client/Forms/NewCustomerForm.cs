using Trackflow.Client.Services;
using Trackflow.Shared.DTOs;

namespace Trackflow.Client.Forms;

public class NewCustomerForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly TextBox _txtFirmaAdi;
    private readonly TextBox _txtGLN;
    private readonly TextBox _txtAciklama;

    public bool IsEditMode { get; }
    public Guid? CustomerId { get; }

    public NewCustomerForm(ApiClient apiClient, CustomerDto? existing = null)
    {
        _apiClient = apiClient;
        IsEditMode = existing != null;
        CustomerId = existing?.Id;

        Text = IsEditMode ? "Müşteri Düzenle" : "Yeni Müşteri";
        Size = new Size(400, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblFirmaAdi = new Label
        {
            Text = "Firma Adı:",
            Location = new Point(20, 20),
            AutoSize = true
        };
        Controls.Add(lblFirmaAdi);

        _txtFirmaAdi = new TextBox
        {
            Location = new Point(20, 45),
            Size = new Size(340, 25),
            Text = existing?.FirmaAdi ?? ""
        };
        Controls.Add(_txtFirmaAdi);

        var lblGLN = new Label
        {
            Text = "GLN (13 hane):",
            Location = new Point(20, 80),
            AutoSize = true
        };
        Controls.Add(lblGLN);

        _txtGLN = new TextBox
        {
            Location = new Point(20, 105),
            Size = new Size(230, 25),
            MaxLength = 13,
            Text = existing?.GLN ?? ""
        };
        Controls.Add(_txtGLN);

        var btnOtomatikUret = new Button
        {
            Text = "Otomatik Üret",
            Location = new Point(260, 103),
            Size = new Size(100, 27)
        };
        btnOtomatikUret.Click += async (s, e) => await GenerateGLNAsync();
        Controls.Add(btnOtomatikUret);

        var lblAciklama = new Label
        {
            Text = "Açıklama:",
            Location = new Point(20, 140),
            AutoSize = true
        };
        Controls.Add(lblAciklama);

        _txtAciklama = new TextBox
        {
            Location = new Point(20, 165),
            Size = new Size(340, 25),
            Text = existing?.Aciklama ?? ""
        };
        Controls.Add(_txtAciklama);

        var btnKaydet = new Button
        {
            Text = "Kaydet",
            Location = new Point(170, 205),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };
        btnKaydet.Click += async (s, e) => await SaveAsync();
        Controls.Add(btnKaydet);

        var btnIptal = new Button
        {
            Text = "İptal",
            Location = new Point(270, 205),
            Size = new Size(90, 30)
        };
        btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(btnIptal);
    }

    private async Task GenerateGLNAsync()
    {
        try
        {
            var gln = await _apiClient.GenerateGLNAsync();
            _txtGLN.Text = gln;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"GLN üretim hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtFirmaAdi.Text))
        {
            MessageBox.Show("Firma adı zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtGLN.Text) || _txtGLN.Text.Length != 13)
        {
            MessageBox.Show("GLN 13 haneli olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var dto = new CreateCustomerDto
            {
                FirmaAdi = _txtFirmaAdi.Text.Trim(),
                GLN = _txtGLN.Text.Trim(),
                Aciklama = string.IsNullOrWhiteSpace(_txtAciklama.Text) ? null : _txtAciklama.Text.Trim()
            };

            if (IsEditMode && CustomerId.HasValue)
            {
                await _apiClient.UpdateCustomerAsync(CustomerId.Value, dto);
            }
            else
            {
                await _apiClient.CreateCustomerAsync(dto);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kayıt hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
