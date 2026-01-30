using Trackflow.Client.Services;
using Trackflow.Shared.DTOs;

namespace Trackflow.Client.Forms;

public class NewProductForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly ComboBox _cmbMusteri;
    private readonly TextBox _txtUrunAdi;
    private readonly TextBox _txtGTIN;
    private List<CustomerDto> _customers = new();

    private readonly ProductDto? _existingProduct;
    public bool IsEditMode => _existingProduct != null;
    public Guid? ProductId => _existingProduct?.Id;

    public NewProductForm(ApiClient apiClient, ProductDto? existing = null)
    {
        _apiClient = apiClient;
        _existingProduct = existing;

        Text = existing == null ? "Yeni Urun" : "Urun Duzenle";
        Size = new Size(400, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblMusteri = new Label
        {
            Text = "Musteri:",
            Location = new Point(20, 20),
            AutoSize = true
        };
        Controls.Add(lblMusteri);

        _cmbMusteri = new ComboBox
        {
            Location = new Point(20, 45),
            Size = new Size(340, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(_cmbMusteri);

        var lblUrunAdi = new Label
        {
            Text = "Urun Adi:",
            Location = new Point(20, 80),
            AutoSize = true
        };
        Controls.Add(lblUrunAdi);

        _txtUrunAdi = new TextBox
        {
            Location = new Point(20, 105),
            Size = new Size(340, 25)
        };
        Controls.Add(_txtUrunAdi);

        var lblGTIN = new Label
        {
            Text = "GTIN (14 hane):",
            Location = new Point(20, 140),
            AutoSize = true
        };
        Controls.Add(lblGTIN);

        _txtGTIN = new TextBox
        {
            Location = new Point(20, 165),
            Size = new Size(340, 25),
            MaxLength = 14
        };
        Controls.Add(_txtGTIN);

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
            Text = "Iptal",
            Location = new Point(270, 205),
            Size = new Size(90, 30)
        };
        btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(btnIptal);

        Load += async (s, e) => await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            _customers = await _apiClient.GetCustomersAsync();
            _cmbMusteri.Items.Clear();
            foreach (var c in _customers)
            {
                _cmbMusteri.Items.Add(c.FirmaAdi);
            }

            if (_existingProduct != null)
            {
                _txtUrunAdi.Text = _existingProduct.UrunAdi;
                _txtGTIN.Text = _existingProduct.GTIN;

                var customerIndex = _customers.FindIndex(c => c.Id == _existingProduct.CustomerId);
                if (customerIndex >= 0)
                    _cmbMusteri.SelectedIndex = customerIndex;
            }
            else if (_cmbMusteri.Items.Count > 0)
            {
                _cmbMusteri.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Musteriler yuklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveAsync()
    {
        if (_cmbMusteri.SelectedIndex < 0)
        {
            MessageBox.Show("Musteri seciniz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtUrunAdi.Text))
        {
            MessageBox.Show("Urun adi zorunludur.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtGTIN.Text) || _txtGTIN.Text.Length != 14)
        {
            MessageBox.Show("GTIN 14 haneli olmalidir.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var selectedCustomer = _customers[_cmbMusteri.SelectedIndex];

            var dto = new CreateProductDto
            {
                CustomerId = selectedCustomer.Id,
                UrunAdi = _txtUrunAdi.Text.Trim(),
                GTIN = _txtGTIN.Text.Trim()
            };

            if (IsEditMode)
            {
                await _apiClient.UpdateProductAsync(_existingProduct!.Id, dto);
            }
            else
            {
                await _apiClient.CreateProductAsync(dto);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kayit hatasi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
