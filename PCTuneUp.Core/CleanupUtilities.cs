using System.Diagnostics;
using System.IO;

namespace PCTuneUp;

/// <summary>
/// Utility class for PC cleanup operations
/// </summary>
public static class CleanupUtilities
{
    /// <summary>
    /// Formats bytes into a human-readable string (GB, MB, KB, or bytes)
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F2} GB";
        if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F2} MB";
        if (bytes >= 1024) return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} bytes";
    }

    /// <summary>
    /// Gets the total size of a directory including all subdirectories
    /// </summary>
    public static long GetDirectorySize(string path)
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

    /// <summary>
    /// Deletes all files in a directory and returns statistics
    /// </summary>
    public static (long cleaned, int skipped) DeleteFilesInDirectoryWithStats(string path)
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

    /// <summary>
    /// Scans temporary files and returns total size
    /// </summary>
    public static long ScanTempFiles()
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
    }

    /// <summary>
    /// Scans Windows Update cache and returns total size
    /// </summary>
    public static long ScanWindowsUpdateCache()
    {
        var path = @"C:\Windows\SoftwareDistribution\Download";
        return Directory.Exists(path) ? GetDirectorySize(path) : 0;
    }

    /// <summary>
    /// Scans Recycle Bin and returns total size
    /// </summary>
    public static long ScanRecycleBin()
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
    }

    /// <summary>
    /// Scans browser cache and returns total size
    /// </summary>
    public static long ScanBrowserCache(string browser)
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
    }

    /// <summary>
    /// Cleans temporary files and returns total bytes cleaned
    /// </summary>
    public static (long cleaned, int skipped) CleanTempFiles()
    {
        long totalCleaned = 0;
        int totalSkipped = 0;
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
                totalSkipped += skipped;
            }
        }
        return (totalCleaned, totalSkipped);
    }

    /// <summary>
    /// Cleans Windows Update cache and returns total bytes cleaned
    /// </summary>
    public static (long cleaned, int skipped) CleanWindowsUpdateCache()
    {
        var path = @"C:\Windows\SoftwareDistribution\Download";
        if (!Directory.Exists(path)) return (0L, 0);
        
        return DeleteFilesInDirectoryWithStats(path);
    }

    /// <summary>
    /// Cleans browser cache and returns total bytes cleaned
    /// </summary>
    public static (long cleaned, int skipped) CleanBrowserCache(string browser)
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
        
        return (totalCleaned, totalSkipped);
    }
}
