using Trackflow.Client.Services;
using Trackflow.Shared.DTOs;
using Trackflow.Shared.Enums;

namespace Trackflow.Client.Forms;

public class MainForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly Label _statusLabel;
    private readonly Panel _contentPanel;

    public MainForm()
    {
        _apiClient = new ApiClient();

        Text = "Trackflow - Serilizasyon Sistemi";
        Size = new Size(1024, 768);
        StartPosition = FormStartPosition.CenterScreen;

        // Menu Panel
        var menuPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 250,
            BackColor = Color.FromArgb(45, 45, 48)
        };

        var titleLabel = new Label
        {
            Text = "TRACKFLOW",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };
        menuPanel.Controls.Add(titleLabel);

        var btnHealth = CreateMenuButton("Sistem Durumu", 80);
        btnHealth.Click += async (s, e) => await ShowHealthAsync();
        menuPanel.Controls.Add(btnHealth);

        var btnCustomers = CreateMenuButton("Müşteriler", 130);
        btnCustomers.Click += async (s, e) => await ShowCustomersAsync();
        menuPanel.Controls.Add(btnCustomers);

        var btnProducts = CreateMenuButton("Ürünler", 180);
        btnProducts.Click += async (s, e) => await ShowProductsAsync();
        menuPanel.Controls.Add(btnProducts);

        var btnWorkOrders = CreateMenuButton("İş Emirleri", 230);
        btnWorkOrders.Click += async (s, e) => await ShowWorkOrdersAsync();
        menuPanel.Controls.Add(btnWorkOrders);

        // Status Bar
        _statusLabel = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            Text = "  Hazır",
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10)
        };

        // Content Panel
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        Controls.Add(_contentPanel);
        Controls.Add(menuPanel);
        Controls.Add(_statusLabel);

        Load += async (s, e) => await ShowHealthAsync();
    }

    private Button CreateMenuButton(string text, int top)
    {
        return new Button
        {
            Text = text,
            Location = new Point(10, top),
            Size = new Size(230, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(62, 62, 66),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand
        };
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = $"  {message}";
    }

    private void ClearContent()
    {
        _contentPanel.Controls.Clear();
    }

    private async Task ShowHealthAsync()
    {
        ClearContent();
        SetStatus("Sistem durumu kontrol ediliyor...");

        try
        {
            var health = await _apiClient.GetHealthAsync();

            var titleLabel = new Label
            {
                Text = "Sistem Durumu",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            _contentPanel.Controls.Add(titleLabel);

            var statusLabel = new Label
            {
                Text = $"Genel Durum: {health?.Status ?? "Bilinmiyor"}",
                Font = new Font("Segoe UI", 14),
                ForeColor = health?.Status == "Healthy" ? Color.Green : Color.Red,
                Location = new Point(20, 70),
                AutoSize = true
            };
            _contentPanel.Controls.Add(statusLabel);

            int y = 120;
            if (health?.Checks != null)
            {
                foreach (var check in health.Checks)
                {
                    var checkLabel = new Label
                    {
                        Text = $"• {check.Name}: {check.Status} ({check.Duration})",
                        Font = new Font("Segoe UI", 11),
                        ForeColor = check.Status == "Healthy" ? Color.DarkGreen : Color.DarkRed,
                        Location = new Point(40, y),
                        AutoSize = true
                    };
                    _contentPanel.Controls.Add(checkLabel);
                    y += 30;

                    if (!string.IsNullOrEmpty(check.Exception))
                    {
                        var exLabel = new Label
                        {
                            Text = $"  Hata: {check.Exception}",
                            Font = new Font("Segoe UI", 9),
                            ForeColor = Color.Red,
                            Location = new Point(60, y),
                            AutoSize = true
                        };
                        _contentPanel.Controls.Add(exLabel);
                        y += 25;
                    }
                }
            }

            SetStatus($"Sistem durumu: {health?.Status}");
        }
        catch (Exception ex)
        {
            ShowError("API bağlantı hatası", ex.Message);
        }
    }

    private async Task ShowCustomersAsync()
    {
        ClearContent();
        SetStatus("Müşteriler yükleniyor...");

        try
        {
            var customers = await _apiClient.GetCustomersAsync();

            var titleLabel = new Label
            {
                Text = "Müşteriler",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            _contentPanel.Controls.Add(titleLabel);

            var btnNewCustomer = new Button
            {
                Text = "+ Yeni Müşteri",
                Location = new Point(_contentPanel.Width - 170, 20),
                Size = new Size(130, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNewCustomer.Click += (s, e) => ShowNewCustomerForm();
            _contentPanel.Controls.Add(btnNewCustomer);

            var btnEdit = new Button
            {
                Text = "Düzenle",
                Location = new Point(_contentPanel.Width - 420, 20),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _contentPanel.Controls.Add(btnEdit);

            var btnDelete = new Button
            {
                Text = "Sil",
                Location = new Point(_contentPanel.Width - 310, 20),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White
            };
            _contentPanel.Controls.Add(btnDelete);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(_contentPanel.Width - 60, _contentPanel.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            grid.Columns.Add("Id", "ID");
            grid.Columns["Id"]!.Visible = false;
            grid.Columns.Add("FirmaAdi", "Firma Adı");
            grid.Columns.Add("GLN", "GLN");
            grid.Columns.Add("Aciklama", "Açıklama");

            foreach (var c in customers)
            {
                grid.Rows.Add(c.Id, c.FirmaAdi, c.GLN, c.Aciklama);
            }

            _contentPanel.Controls.Add(grid);

            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    var row = grid.SelectedRows[0];
                    var customer = new CustomerDto
                    {
                        Id = Guid.Parse(row.Cells["Id"].Value?.ToString() ?? ""),
                        FirmaAdi = row.Cells["FirmaAdi"].Value?.ToString() ?? "",
                        GLN = row.Cells["GLN"].Value?.ToString() ?? "",
                        Aciklama = row.Cells["Aciklama"].Value?.ToString()
                    };
                    var form = new NewCustomerForm(_apiClient, customer);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _ = ShowCustomersAsync();
                    }
                }
            };

            btnDelete.Click += async (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    var id = Guid.Parse(grid.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "");
                    var firmaAdi = grid.SelectedRows[0].Cells["FirmaAdi"].Value?.ToString();

                    var result = MessageBox.Show(
                        $"\"{firmaAdi}\" müşterisini silmek istediğinizden emin misiniz?",
                        "Silme Onayı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            await _apiClient.DeleteCustomerAsync(id);
                            await ShowCustomersAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            SetStatus($"{customers.Count} müşteri listelendi");
        }
        catch (Exception ex)
        {
            ShowError("Müşteriler yüklenemedi", ex.Message);
        }
    }

    private async Task ShowProductsAsync()
    {
        ClearContent();
        SetStatus("Ürünler yükleniyor...");

        try
        {
            var products = await _apiClient.GetProductsAsync();

            var titleLabel = new Label
            {
                Text = "Ürünler",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            _contentPanel.Controls.Add(titleLabel);

            var btnNewProduct = new Button
            {
                Text = "+ Yeni Ürün",
                Location = new Point(_contentPanel.Width - 170, 20),
                Size = new Size(130, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNewProduct.Click += (s, e) => ShowNewProductForm();
            _contentPanel.Controls.Add(btnNewProduct);

            var btnEdit = new Button
            {
                Text = "Düzenle",
                Location = new Point(_contentPanel.Width - 420, 20),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _contentPanel.Controls.Add(btnEdit);

            var btnDelete = new Button
            {
                Text = "Sil",
                Location = new Point(_contentPanel.Width - 310, 20),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White
            };
            _contentPanel.Controls.Add(btnDelete);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(_contentPanel.Width - 60, _contentPanel.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            grid.Columns.Add("Id", "ID");
            grid.Columns["Id"]!.Visible = false;
            grid.Columns.Add("CustomerId", "CustomerId");
            grid.Columns["CustomerId"]!.Visible = false;
            grid.Columns.Add("UrunAdi", "Ürün Adı");
            grid.Columns.Add("GTIN", "GTIN");
            grid.Columns.Add("CustomerName", "Müşteri");

            foreach (var p in products)
            {
                grid.Rows.Add(p.Id, p.CustomerId, p.UrunAdi, p.GTIN, p.CustomerName);
            }

            _contentPanel.Controls.Add(grid);

            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    var row = grid.SelectedRows[0];
                    var product = new ProductDto
                    {
                        Id = Guid.Parse(row.Cells["Id"].Value?.ToString() ?? ""),
                        CustomerId = Guid.Parse(row.Cells["CustomerId"].Value?.ToString() ?? ""),
                        UrunAdi = row.Cells["UrunAdi"].Value?.ToString() ?? "",
                        GTIN = row.Cells["GTIN"].Value?.ToString() ?? "",
                        CustomerName = row.Cells["CustomerName"].Value?.ToString() ?? ""
                    };
                    var form = new NewProductForm(_apiClient, product);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _ = ShowProductsAsync();
                    }
                }
            };

            btnDelete.Click += async (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    var id = Guid.Parse(grid.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "");
                    var urunAdi = grid.SelectedRows[0].Cells["UrunAdi"].Value?.ToString();

                    var result = MessageBox.Show(
                        $"\"{urunAdi}\" ürününü silmek istediğinizden emin misiniz?",
                        "Silme Onayı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            await _apiClient.DeleteProductAsync(id);
                            await ShowProductsAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            SetStatus($"{products.Count} ürün listelendi");
        }
        catch (Exception ex)
        {
            ShowError("Ürünler yüklenemedi", ex.Message);
        }
    }

    private async Task ShowWorkOrdersAsync()
    {
        ClearContent();
        SetStatus("İş emirleri yükleniyor...");

        try
        {
            var workOrders = await _apiClient.GetWorkOrdersAsync();

            var titleLabel = new Label
            {
                Text = "İş Emirleri",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            _contentPanel.Controls.Add(titleLabel);

            var btnNewWorkOrder = new Button
            {
                Text = "+ Yeni İş Emri",
                Location = new Point(_contentPanel.Width - 170, 20),
                Size = new Size(130, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNewWorkOrder.Click += (s, e) => ShowNewWorkOrderForm();
            _contentPanel.Controls.Add(btnNewWorkOrder);

            var btnDeleteWorkOrder = new Button
            {
                Text = "Sil",
                Location = new Point(_contentPanel.Width - 310, 20),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White
            };
            _contentPanel.Controls.Add(btnDeleteWorkOrder);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(_contentPanel.Width - 60, _contentPanel.Height - 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            grid.Columns.Add("Id", "ID");
            grid.Columns["Id"]!.Visible = false;
            grid.Columns.Add("ProductName", "Ürün");
            grid.Columns.Add("LotNo", "Lot No");
            grid.Columns.Add("Miktar", "Miktar");
            grid.Columns.Add("SKT", "Son Kullanma");
            grid.Columns.Add("Durum", "Durum");

            string[] durumlar = { "Oluşturuldu", "Devam Ediyor", "Tamamlandı", "İptal" };

            foreach (var w in workOrders)
            {
                var durumInt = (int)w.Durum;
                var durum = durumInt >= 0 && durumInt < durumlar.Length ? durumlar[durumInt] : "?";
                grid.Rows.Add(w.Id, w.ProductName, w.LotNo, w.Miktar, w.SonKullanmaTarihi.ToString("dd.MM.yyyy"), durum);
            }

            _contentPanel.Controls.Add(grid);

            var btnDetail = new Button
            {
                Text = "Detay Görüntüle",
                Location = new Point(20, _contentPanel.Height - 60),
                Size = new Size(150, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnDetail.Click += async (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    var id = Guid.Parse(grid.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "");
                    await ShowWorkOrderDetailAsync(id);
                }
            };
            _contentPanel.Controls.Add(btnDetail);

            var btnAggregate = new Button
            {
                Text = "Agregasyon Yap",
                Location = new Point(180, _contentPanel.Height - 60),
                Size = new Size(150, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };
            btnAggregate.Click += async (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    var id = Guid.Parse(grid.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "");
                    await AggregateWorkOrderAsync(id);
                }
            };
            _contentPanel.Controls.Add(btnAggregate);

            btnDeleteWorkOrder.Click += async (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    var id = Guid.Parse(grid.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "");
                    var lotNo = grid.SelectedRows[0].Cells["LotNo"].Value?.ToString();

                    var result = MessageBox.Show(
                        $"\"{lotNo}\" iş emrini silmek istediğinizden emin misiniz?\n\nBu işlem tüm seri numaralarını ve paketleme birimlerini de silecektir!",
                        "Silme Onayı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            await _apiClient.DeleteWorkOrderAsync(id);
                            await ShowWorkOrdersAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            SetStatus($"{workOrders.Count} iş emri listelendi");
        }
        catch (Exception ex)
        {
            ShowError("İş emirleri yüklenemedi", ex.Message);
        }
    }

    private async Task ShowWorkOrderDetailAsync(Guid id)
    {
        ClearContent();
        SetStatus("İş emri detayı yükleniyor...");

        try
        {
            var detail = await _apiClient.GetWorkOrderDetailAsync(id);
            if (detail == null)
            {
                ShowError("Hata", "İş emri bulunamadı");
                return;
            }

            var titleLabel = new Label
            {
                Text = $"İş Emri Detayı - {detail.LotNo}",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            _contentPanel.Controls.Add(titleLabel);

            var infoPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(_contentPanel.Width - 60, 120),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var info = $"Ürün: {detail.Product.UrunAdi} | GTIN: {detail.Product.GTIN}\n" +
                       $"Müşteri: {detail.Customer.FirmaAdi} | GLN: {detail.Customer.GLN}\n" +
                       $"Miktar: {detail.Miktar} | Lot: {detail.LotNo} | SKT: {detail.SonKullanmaTarihi:dd.MM.yyyy}\n" +
                       $"Toplam Seri: {detail.TotalSerials} | Agrege: {detail.AggregatedSerials} | Koli: {detail.TotalBoxes} | Palet: {detail.TotalPallets}";

            var infoLabel = new Label
            {
                Text = info,
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            infoPanel.Controls.Add(infoLabel);
            _contentPanel.Controls.Add(infoPanel);

            // Serial Numbers Grid
            var serialLabel = new Label
            {
                Text = "Seri Numaraları",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 200),
                AutoSize = true
            };
            _contentPanel.Controls.Add(serialLabel);

            var serialGrid = new DataGridView
            {
                Location = new Point(20, 230),
                Size = new Size(_contentPanel.Width - 60, _contentPanel.Height - 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            serialGrid.Columns.Add("SeriNo", "Seri No");
            serialGrid.Columns.Add("GS1Barkod", "GS1 Barkod");
            serialGrid.Columns.Add("Durum", "Durum");

            string[] durumlar = { "Üretildi", "Basıldı", "Doğrulandı", "Reddedildi", "Agrege" };

            foreach (var s in detail.SerialNumbers.Take(100)) // İlk 100
            {
                var durumInt = (int)s.Durum;
                var durum = durumInt >= 0 && durumInt < durumlar.Length ? durumlar[durumInt] : "?";
                serialGrid.Rows.Add(s.SeriNo, s.GS1Barkod, durum);
            }

            _contentPanel.Controls.Add(serialGrid);

            var btnBack = new Button
            {
                Text = "< Geri",
                Location = new Point(20, _contentPanel.Height - 50),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnBack.Click += async (s, e) => await ShowWorkOrdersAsync();
            _contentPanel.Controls.Add(btnBack);

            SetStatus($"İş emri detayı: {detail.TotalSerials} seri numarası");
        }
        catch (Exception ex)
        {
            ShowError("Detay yüklenemedi", ex.Message);
        }
    }

    private async Task AggregateWorkOrderAsync(Guid id)
    {
        SetStatus("Agregasyon yapılıyor...");

        try
        {
            var result = await _apiClient.AggregateWorkOrderAsync(id);
            MessageBox.Show(
                $"Agregasyon tamamlandı!\n\n" +
                $"Toplam Seri: {result?.TotalSerials}\n" +
                $"Oluşturulan Koli: {result?.TotalBoxes}\n" +
                $"Oluşturulan Palet: {result?.TotalPallets}",
                "Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await ShowWorkOrdersAsync();
        }
        catch (Exception ex)
        {
            ShowError("Agregasyon başarısız", ex.Message);
        }
    }

    private void ShowNewWorkOrderForm()
    {
        var form = new NewWorkOrderForm(_apiClient);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _ = ShowWorkOrdersAsync();
        }
    }

    private void ShowNewCustomerForm()
    {
        var form = new NewCustomerForm(_apiClient);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _ = ShowCustomersAsync();
        }
    }

    private void ShowNewProductForm()
    {
        var form = new NewProductForm(_apiClient);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _ = ShowProductsAsync();
        }
    }

    private void ShowError(string title, string message)
    {
        SetStatus($"Hata: {message}");
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
