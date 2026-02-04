using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCTuneUp;

public partial class MainWindow : Window
{
    private readonly List<ScanResult> _scanResults = new();
    private readonly Dictionary<string, CheckBox> _checkboxes = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private class ScanResult
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public long SizeBytes { get; set; }
        public string Description { get; set; } = "";
        public Func<Action<string>, Task<long>>? CleanAction { get; set; }
    }

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        btnScan.IsEnabled = false;
        btnFix.IsEnabled = false;
        btnSelectAll.IsEnabled = false;
        btnScan.Content = "🔍 Scanning...";
        txtStatus.Text = "Scanning your system...";
        progressBar.Value = 0;
        txtLog.Text = "";
        _scanResults.Clear();
        _checkboxes.Clear();
        pnlScanResults.Children.Clear();
        txtPlaceholder.Visibility = Visibility.Collapsed;

        try
        {
            var scanTasks = new (string Id, string Name, string Icon, string Desc, Func<Task<long>> ScanFunc, Func<Action<string>, Task<long>> CleanFunc)[]
            {
                ("temp", "Windows Temp Files", "🗂️", "Temporary files from Windows and applications", ScanTempFilesAsync, CleanTempFilesAsync),
                ("wuCache", "Windows Update Cache", "📦", "Old Windows Update download files", ScanWindowsUpdateCacheAsync, CleanWindowsUpdateCacheAsync),
                ("recycle", "Recycle Bin", "🗑️", "Deleted files waiting to be permanently removed", ScanRecycleBinAsync, CleanRecycleBinAsync),
                ("chromeCache", "Chrome Browser Cache", "🌐", "Cached data (close Chrome first!)", () => ScanBrowserCacheAsync("Chrome"), log => CleanBrowserCacheAsync("Chrome", log)),
                ("edgeCache", "Edge Browser Cache", "🌐", "Cached data (close Edge first!)", () => ScanBrowserCacheAsync("Edge"), log => CleanBrowserCacheAsync("Edge", log)),
                ("firefoxCache", "Firefox Browser Cache", "🦊", "Cached data (close Firefox first!)", () => ScanBrowserCacheAsync("Firefox"), log => CleanBrowserCacheAsync("Firefox", log)),
                ("dns", "DNS Cache", "🔗", "Flush to resolve connectivity issues", ScanDnsCacheAsync, CleanDnsCacheAsync),
            };

            double progressStep = 100.0 / scanTasks.Length;
            double currentProgress = 0;

            foreach (var task in scanTasks)
            {
                Log($"Scanning {task.Name}...");
                var size = await task.ScanFunc();
                
                if (size > 0 || task.Id == "dns")
                {
                    _scanResults.Add(new ScanResult
                    {
                        Id = task.Id,
                        Name = task.Name,
                        Icon = task.Icon,
                        SizeBytes = size,
                        Description = task.Desc,
                        CleanAction = task.CleanFunc
                    });
                }
                
                currentProgress += progressStep;
                progressBar.Value = currentProgress;
            }

            DisplayScanResults();
            
            long totalSize = _scanResults.Sum(r => r.SizeBytes);
            txtTotalSize.Text = totalSize > 0 ? FormatBytes(totalSize) : "System is clean!";
            txtStatus.Text = $"Scan complete. Found {_scanResults.Count} items to clean.";
            progressBar.Value = 100;
            
            if (_scanResults.Count > 0)
            {
                btnFix.IsEnabled = true;
                btnSelectAll.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
            txtStatus.Text = "Scan failed. See log for details.";
        }
        finally
        {
            btnScan.IsEnabled = true;
            btnScan.Content = "🔍 Scan";
        }
    }

    private void DisplayScanResults()
    {
        pnlScanResults.Children.Clear();
        _checkboxes.Clear();

        foreach (var result in _scanResults)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#45475a")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 6, 0, 0)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var checkbox = new CheckBox
            {
                IsChecked = true,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            checkbox.Checked += (s, e) => UpdateSelectedSize();
            checkbox.Unchecked += (s, e) => UpdateSelectedSize();
            _checkboxes[result.Id] = checkbox;
            Grid.SetColumn(checkbox, 0);

            var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            infoPanel.Children.Add(new TextBlock
            {
                Text = $"{result.Icon} {result.Name}",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cdd6f4")),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            });
            infoPanel.Children.Add(new TextBlock
            {
                Text = result.Description,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c7086")),
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(infoPanel, 1);

            var sizeText = new TextBlock
            {
                Text = result.SizeBytes > 0 ? FormatBytes(result.SizeBytes) : "Available",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(result.SizeBytes > 100_000_000 ? "#f38ba8" : "#f9e2af")),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(sizeText, 2);

            grid.Children.Add(checkbox);
            grid.Children.Add(infoPanel);
            grid.Children.Add(sizeText);
            border.Child = grid;
            pnlScanResults.Children.Add(border);
        }

        UpdateSelectedSize();
    }

    private void UpdateSelectedSize()
    {
        long selectedSize = _scanResults
            .Where(r => _checkboxes.ContainsKey(r.Id) && _checkboxes[r.Id].IsChecked == true)
            .Sum(r => r.SizeBytes);
        
        int selectedCount = _checkboxes.Count(c => c.Value.IsChecked == true);
        txtSelectedSize.Text = selectedCount > 0 ? $"Selected: {FormatBytes(selectedSize)}" : "";
        btnFix.IsEnabled = selectedCount > 0;
    }

    private async void BtnFix_Click(object sender, RoutedEventArgs e)
    {
        // Check if browsers are running
        var selectedIds = _scanResults
            .Where(r => _checkboxes.ContainsKey(r.Id) && _checkboxes[r.Id].IsChecked == true)
            .Select(r => r.Id)
            .ToList();

        var browserWarnings = new List<string>();
        if (selectedIds.Contains("chromeCache") && Process.GetProcessesByName("chrome").Length > 0)
            browserWarnings.Add("Chrome");
        if (selectedIds.Contains("edgeCache") && Process.GetProcessesByName("msedge").Length > 0)
            browserWarnings.Add("Edge");
        if (selectedIds.Contains("firefoxCache") && Process.GetProcessesByName("firefox").Length > 0)
            browserWarnings.Add("Firefox");

        if (browserWarnings.Count > 0)
        {
            var result = MessageBox.Show(
                $"The following browsers are running and their cache cannot be fully cleaned:\n\n• {string.Join("\n• ", browserWarnings)}\n\nWould you like to close them automatically?",
                "Browsers Running",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Log("Closing browsers...");
                foreach (var browser in browserWarnings)
                {
                    var processName = browser switch
                    {
                        "Chrome" => "chrome",
                        "Edge" => "msedge",
                        "Firefox" => "firefox",
                        _ => null
                    };
                    if (processName != null)
                    {
                        foreach (var proc in Process.GetProcessesByName(processName))
                        {
                            try { proc.Kill(); proc.WaitForExit(3000); } catch { }
                        }
                    }
                }
                await Task.Delay(2000); // Wait for processes to fully close
                Log("Browsers closed.");
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        btnScan.IsEnabled = false;
        btnFix.IsEnabled = false;
        btnSelectAll.IsEnabled = false;
        btnFix.Content = "⏳ Fixing...";
        txtStatus.Text = "Cleaning selected items...";
        progressBar.Value = 0;
        txtLog.Text = "";

        try
        {
            var selectedResults = _scanResults
                .Where(r => _checkboxes.ContainsKey(r.Id) && _checkboxes[r.Id].IsChecked == true)
                .ToList();

            double progressStep = 100.0 / selectedResults.Count;
            double currentProgress = 0;
            long totalCleaned = 0;
            int successCount = 0;
            int failCount = 0;

            foreach (var scanResult in selectedResults)
            {
                Log($"\n▶ Cleaning {scanResult.Name}...");
                if (scanResult.CleanAction != null)
                {
                    try
                    {
                        var cleaned = await scanResult.CleanAction(Log);
                        totalCleaned += cleaned;
                        if (cleaned > 0 || scanResult.Id == "dns")
                        {
                            Log($"   ✓ Cleaned {FormatBytes(cleaned)}");
                            successCount++;
                        }
                        else
                        {
                            Log($"   ⚠ Could not clean (files may be in use)");
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"   ✗ Error: {ex.Message}");
                        failCount++;
                    }
                }
                currentProgress += progressStep;
                progressBar.Value = currentProgress;
            }

            progressBar.Value = 100;
            
            var statusMsg = $"✅ Done! Freed {FormatBytes(totalCleaned)}";
            if (failCount > 0)
                statusMsg += $" ({failCount} items skipped - files in use)";
            
            txtStatus.Text = statusMsg;
            txtTotalSize.Text = $"Cleaned: {FormatBytes(totalCleaned)}";
            Log($"\n{'=',-40}");
            Log($"✅ Total space recovered: {FormatBytes(totalCleaned)}");
            if (failCount > 0)
                Log($"⚠ {failCount} items could not be fully cleaned (close apps and retry)");

            _scanResults.Clear();
            pnlScanResults.Children.Clear();
            txtPlaceholder.Text = "Click Scan to check again.";
            txtPlaceholder.Visibility = Visibility.Visible;
            pnlScanResults.Children.Add(txtPlaceholder);
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
            txtStatus.Text = "Cleanup failed. See log for details.";
        }
        finally
        {
            btnScan.IsEnabled = true;
            btnFix.IsEnabled = false;
            btnSelectAll.IsEnabled = false;
            btnFix.Content = "🚀 Fix Selected";
        }
    }

    private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
    {
        bool allChecked = _checkboxes.Values.All(c => c.IsChecked == true);
        foreach (var cb in _checkboxes.Values)
            cb.IsChecked = !allChecked;
        btnSelectAll.Content = allChecked ? "Select All" : "Deselect All";
    }

    private void Log(string message)
    {
        Dispatcher.Invoke(() =>
        {
            txtLog.Text += message + "\n";
            logScroller.ScrollToEnd();
        });
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F2} GB";
        if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F2} MB";
        if (bytes >= 1024) return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} bytes";
    }

    // ===== SCAN METHODS =====
    
    private Task<long> ScanTempFilesAsync() => Task.Run(() =>
    {
        long totalSize = 0;
        var tempPaths = new[]
        {
            Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
        };

        foreach (var path in tempPaths)
        {
            if (Directory.Exists(path))
                totalSize += GetDirectorySize(path);
        }
        return totalSize;
    });

    private Task<long> ScanWindowsUpdateCacheAsync() => Task.Run(() =>
    {
        var path = @"C:\Windows\SoftwareDistribution\Download";
        return Directory.Exists(path) ? GetDirectorySize(path) : 0;
    });

    private Task<long> ScanRecycleBinAsync() => Task.Run(() =>
    {
        long totalSize = 0;
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            var recyclePath = Path.Combine(drive.RootDirectory.FullName, "$Recycle.Bin");
            if (Directory.Exists(recyclePath))
            {
                try { totalSize += GetDirectorySize(recyclePath); } catch { }
            }
        }
        return totalSize;
    });

    private Task<long> ScanBrowserCacheAsync(string browser) => Task.Run(() =>
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        var cachePaths = browser switch
        {
            "Chrome" => new[] { 
                Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache"),
                Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Code Cache"),
                Path.Combine(localAppData, @"Google\Chrome\User Data\Default\GPUCache")
            },
            "Edge" => new[] { 
                Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache"),
                Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Code Cache"),
                Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\GPUCache")
            },
            "Firefox" => new[] { Path.Combine(localAppData, @"Mozilla\Firefox\Profiles") },
            _ => Array.Empty<string>()
        };

        long total = 0;
        foreach (var cachePath in cachePaths)
        {
            if (!Directory.Exists(cachePath)) continue;

            if (browser == "Firefox")
            {
                foreach (var profile in Directory.GetDirectories(cachePath))
                {
                    var cache2 = Path.Combine(profile, "cache2");
                    if (Directory.Exists(cache2))
                        total += GetDirectorySize(cache2);
                }
            }
            else
            {
                total += GetDirectorySize(cachePath);
            }
        }
        return total;
    });

    private Task<long> ScanDnsCacheAsync() => Task.FromResult(0L);

    // ===== CLEAN METHODS =====

    private Task<long> CleanTempFilesAsync(Action<string> log) => Task.Run(() =>
    {
        long totalCleaned = 0;
        var tempPaths = new[]
        {
            Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
        };

        foreach (var path in tempPaths)
        {
            if (Directory.Exists(path))
            {
                var (cleaned, skipped) = DeleteFilesInDirectoryWithStats(path);
                totalCleaned += cleaned;
                if (skipped > 0)
                    log($"   Skipped {skipped} files in use");
            }
        }
        return totalCleaned;
    });

    private Task<long> CleanWindowsUpdateCacheAsync(Action<string> log) => Task.Run(() =>
    {
        var path = @"C:\Windows\SoftwareDistribution\Download";
        if (!Directory.Exists(path)) return 0L;
        
        var (cleaned, skipped) = DeleteFilesInDirectoryWithStats(path);
        if (skipped > 0)
            log($"   Skipped {skipped} files in use");
        return cleaned;
    });

    private Task<long> CleanRecycleBinAsync(Action<string> log) => Task.Run(() =>
    {
        long sizeBefore = 0;
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            var recyclePath = Path.Combine(drive.RootDirectory.FullName, "$Recycle.Bin");
            if (Directory.Exists(recyclePath))
            {
                try { sizeBefore += GetDirectorySize(recyclePath); } catch { }
            }
        }
        
        // SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND
        SHEmptyRecycleBin(IntPtr.Zero, null, 0x00000007);
        return sizeBefore;
    });

    [System.Runtime.InteropServices.DllImport("Shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    private Task<long> CleanBrowserCacheAsync(string browser, Action<string> log) => Task.Run(() =>
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        var cachePaths = browser switch
        {
            "Chrome" => new[] { 
                Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache"),
                Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Code Cache"),
                Path.Combine(localAppData, @"Google\Chrome\User Data\Default\GPUCache")
            },
            "Edge" => new[] { 
                Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache"),
                Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Code Cache"),
                Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\GPUCache")
            },
            "Firefox" => new[] { Path.Combine(localAppData, @"Mozilla\Firefox\Profiles") },
            _ => Array.Empty<string>()
        };

        long totalCleaned = 0;
        int totalSkipped = 0;
        
        foreach (var cachePath in cachePaths)
        {
            if (!Directory.Exists(cachePath)) continue;

            if (browser == "Firefox")
            {
                foreach (var profile in Directory.GetDirectories(cachePath))
                {
                    var cache2 = Path.Combine(profile, "cache2");
                    if (Directory.Exists(cache2))
                    {
                        var (cleaned, skipped) = DeleteFilesInDirectoryWithStats(cache2);
                        totalCleaned += cleaned;
                        totalSkipped += skipped;
                    }
                }
            }
            else
            {
                var (cleaned, skipped) = DeleteFilesInDirectoryWithStats(cachePath);
                totalCleaned += cleaned;
                totalSkipped += skipped;
            }
        }
        
        if (totalSkipped > 0)
            log($"   Skipped {totalSkipped} files (browser may still be running)");
            
        return totalCleaned;
    });

    private Task<long> CleanDnsCacheAsync(Action<string> log) => Task.Run(() =>
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ipconfig",
            Arguments = "/flushdns",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        var proc = Process.Start(psi);
        proc?.WaitForExit();
        return 0L;
    });

    // ===== HELPERS =====

    private static long GetDirectorySize(string path)
    {
        long size = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try { size += new FileInfo(file).Length; } catch { }
            }
        }
        catch { }
        return size;
    }

    private static (long cleaned, int skipped) DeleteFilesInDirectoryWithStats(string path)
    {
        long totalCleaned = 0;
        int skippedCount = 0;
        
        try
        {
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var fi = new FileInfo(file);
                    var size = fi.Length;
                    fi.Delete();
                    totalCleaned += size;
                }
                catch
                {
                    skippedCount++;
                }
            }
            
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }
        catch { }
        
        return (totalCleaned, skippedCount);
    }
}