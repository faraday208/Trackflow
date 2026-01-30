using Trackflow.Client.Services;
using Trackflow.Shared.DTOs;

namespace Trackflow.Client.Forms;

public class NewWorkOrderForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly ComboBox _productCombo;
    private readonly NumericUpDown _miktarInput;
    private readonly TextBox _lotNoInput;
    private readonly DateTimePicker _sktPicker;
    private readonly NumericUpDown _seriBaslangicInput;
    private readonly NumericUpDown _koliKapasitesiInput;
    private readonly NumericUpDown _paletKapasitesiInput;

    public NewWorkOrderForm(ApiClient apiClient)
    {
        _apiClient = apiClient;

        Text = "Yeni İş Emri Oluştur";
        Size = new Size(450, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var y = 20;
        var labelWidth = 120;
        var inputX = 140;
        var inputWidth = 270;

        // Ürün
        AddLabel("Ürün:", y);
        _productCombo = new ComboBox
        {
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(_productCombo);
        y += 40;

        // Miktar
        AddLabel("Miktar:", y);
        _miktarInput = new NumericUpDown
        {
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 25),
            Minimum = 1,
            Maximum = 100000,
            Value = 100
        };
        Controls.Add(_miktarInput);
        y += 40;

        // Lot No
        AddLabel("Lot No:", y);
        _lotNoInput = new TextBox
        {
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 25),
            Text = $"LOT{DateTime.Now:yyyyMMdd}"
        };
        Controls.Add(_lotNoInput);
        y += 40;

        // SKT
        AddLabel("Son Kullanma:", y);
        _sktPicker = new DateTimePicker
        {
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 25),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddYears(2)
        };
        Controls.Add(_sktPicker);
        y += 40;

        // Seri Başlangıç
        AddLabel("Seri Başlangıç:", y);
        _seriBaslangicInput = new NumericUpDown
        {
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 25),
            Minimum = 1,
            Maximum = 999999999,
            Value = 1
        };
        Controls.Add(_seriBaslangicInput);
        y += 40;

        // Koli Kapasitesi
        AddLabel("Koli Kapasitesi:", y);
        _koliKapasitesiInput = new NumericUpDown
        {
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 25),
            Minimum = 1,
            Maximum = 1000,
            Value = 10
        };
        Controls.Add(_koliKapasitesiInput);
        y += 40;

        // Palet Kapasitesi
        AddLabel("Palet Kapasitesi:", y);
        _paletKapasitesiInput = new NumericUpDown
        {
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 25),
            Minimum = 1,
            Maximum = 100,
            Value = 10
        };
        Controls.Add(_paletKapasitesiInput);
        y += 50;

        // Buttons
        var btnCreate = new Button
        {
            Text = "Oluştur",
            Location = new Point(inputX, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };
        btnCreate.Click += BtnCreate_Click;
        Controls.Add(btnCreate);

        var btnCancel = new Button
        {
            Text = "İptal",
            Location = new Point(inputX + 110, y),
            Size = new Size(100, 35)
        };
        btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(btnCancel);

        Load += async (s, e) => await LoadProductsAsync();
    }

    private void AddLabel(string text, int y)
    {
        var label = new Label
        {
            Text = text,
            Location = new Point(20, y + 3),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };
        Controls.Add(label);
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products = await _apiClient.GetProductsAsync();
            _productCombo.Items.Clear();
            foreach (var p in products)
            {
                _productCombo.Items.Add(new ProductItem(p.Id, $"{p.UrunAdi} ({p.GTIN})"));
            }
            if (_productCombo.Items.Count > 0)
                _productCombo.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ürünler yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (_productCombo.SelectedItem == null)
        {
            MessageBox.Show("Lütfen bir ürün seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_lotNoInput.Text))
        {
            MessageBox.Show("Lütfen lot numarası girin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var productItem = (ProductItem)_productCombo.SelectedItem;
            var dto = new CreateWorkOrderDto
            {
                ProductId = productItem.Id,
                Miktar = (int)_miktarInput.Value,
                LotNo = _lotNoInput.Text,
                SonKullanmaTarihi = _sktPicker.Value,
                SeriBaslangic = (int)_seriBaslangicInput.Value,
                KoliKapasitesi = (int)_koliKapasitesiInput.Value,
                PaletKapasitesi = (int)_paletKapasitesiInput.Value
            };

            var result = await _apiClient.CreateWorkOrderAsync(dto);

            MessageBox.Show(
                $"İş emri oluşturuldu!\n\n" +
                $"Lot No: {result?.LotNo}\n" +
                $"Miktar: {result?.Miktar} adet seri numarası üretildi",
                "Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"İş emri oluşturulamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private class ProductItem
    {
        public Guid Id { get; }
        public string DisplayText { get; }

        public ProductItem(Guid id, string displayText)
        {
            Id = id;
            DisplayText = displayText;
        }

        public override string ToString() => DisplayText;
    }
}
