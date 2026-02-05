namespace PCTuneUp.Tests;

/// <summary>
/// Unit tests for the CleanupUtilities class
/// </summary>
public class CleanupUtilitiesTests
{
    [Fact]
    public void FormatBytes_ZeroBytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(0);

        // Assert
        Assert.Equal("0 bytes", result);
    }

    [Fact]
    public void FormatBytes_SmallBytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(512);

        // Assert
        Assert.Equal("512 bytes", result);
    }

    [Fact]
    public void FormatBytes_Kilobytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(1024);

        // Assert
        Assert.Equal("1.00 KB", result);
    }

    [Fact]
    public void FormatBytes_MultipleKilobytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(2560); // 2.5 KB

        // Assert
        Assert.Equal("2.50 KB", result);
    }

    [Fact]
    public void FormatBytes_Megabytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(1_048_576);

        // Assert
        Assert.Equal("1.00 MB", result);
    }

    [Fact]
    public void FormatBytes_MultipleMegabytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(52_428_800); // 50 MB

        // Assert
        Assert.Equal("50.00 MB", result);
    }

    [Fact]
    public void FormatBytes_Gigabytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(1_073_741_824);

        // Assert
        Assert.Equal("1.00 GB", result);
    }

    [Fact]
    public void FormatBytes_MultipleGigabytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(5_368_709_120); // 5 GB

        // Assert
        Assert.Equal("5.00 GB", result);
    }

    [Fact]
    public void FormatBytes_FractionalGigabytes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var result = CleanupUtilities.FormatBytes(1_610_612_736); // 1.5 GB

        // Assert
        Assert.Equal("1.50 GB", result);
    }

    [Fact]
    public void GetDirectorySize_NonExistentDirectory_ReturnsZero()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = CleanupUtilities.GetDirectorySize(nonExistentPath);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetDirectorySize_EmptyDirectory_ReturnsZero()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"PCTuneUpTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = CleanupUtilities.GetDirectorySize(tempDir);

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetDirectorySize_DirectoryWithFiles_ReturnsCorrectSize()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"PCTuneUpTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create test files
            var file1 = Path.Combine(tempDir, "file1.txt");
            var file2 = Path.Combine(tempDir, "file2.txt");
            File.WriteAllText(file1, "Hello"); // 5 bytes
            File.WriteAllText(file2, "World!"); // 6 bytes

            // Act
            var result = CleanupUtilities.GetDirectorySize(tempDir);

            // Assert
            Assert.Equal(11, result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetDirectorySize_DirectoryWithSubdirectories_ReturnsCorrectSize()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"PCTuneUpTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "subdir");
        Directory.CreateDirectory(subDir);

        try
        {
            // Create test files
            var file1 = Path.Combine(tempDir, "file1.txt");
            var file2 = Path.Combine(subDir, "file2.txt");
            File.WriteAllText(file1, "Test"); // 4 bytes
            File.WriteAllText(file2, "Data"); // 4 bytes

            // Act
            var result = CleanupUtilities.GetDirectorySize(tempDir);

            // Assert
            Assert.Equal(8, result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeleteFilesInDirectoryWithStats_EmptyDirectory_ReturnsZeroCleaned()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"PCTuneUpTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var (cleaned, skipped) = CleanupUtilities.DeleteFilesInDirectoryWithStats(tempDir);

            // Assert
            Assert.Equal(0, cleaned);
            Assert.Equal(0, skipped);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeleteFilesInDirectoryWithStats_DirectoryWithFiles_DeletesFilesAndReturnsSize()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"PCTuneUpTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create test files
            var file1 = Path.Combine(tempDir, "file1.txt");
            var file2 = Path.Combine(tempDir, "file2.txt");
            File.WriteAllText(file1, "Hello"); // 5 bytes
            File.WriteAllText(file2, "World!"); // 6 bytes

            // Act
            var (cleaned, skipped) = CleanupUtilities.DeleteFilesInDirectoryWithStats(tempDir);

            // Assert
            Assert.Equal(11, cleaned);
            Assert.Equal(0, skipped);
            Assert.False(File.Exists(file1));
            Assert.False(File.Exists(file2));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeleteFilesInDirectoryWithStats_DirectoryWithSubdirectories_DeletesAllContent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"PCTuneUpTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "subdir");
        Directory.CreateDirectory(subDir);

        try
        {
            // Create test files
            var file1 = Path.Combine(tempDir, "file1.txt");
            var file2 = Path.Combine(subDir, "file2.txt");
            File.WriteAllText(file1, "Test"); // 4 bytes
            File.WriteAllText(file2, "Data"); // 4 bytes

            // Act
            var (cleaned, skipped) = CleanupUtilities.DeleteFilesInDirectoryWithStats(tempDir);

            // Assert
            Assert.Equal(8, cleaned);
            Assert.False(File.Exists(file1));
            Assert.False(File.Exists(file2));
            Assert.False(Directory.Exists(subDir));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeleteFilesInDirectoryWithStats_NonExistentDirectory_ReturnsZero()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var (cleaned, skipped) = CleanupUtilities.DeleteFilesInDirectoryWithStats(nonExistentPath);

        // Assert
        Assert.Equal(0, cleaned);
        Assert.Equal(0, skipped);
    }

    [Fact]
    public void ScanBrowserCache_UnknownBrowser_ReturnsZero()
    {
        // Act
        var result = CleanupUtilities.ScanBrowserCache("UnknownBrowser");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void ScanBrowserCache_Chrome_ReturnsNonNegativeValue()
    {
        // Act
        var result = CleanupUtilities.ScanBrowserCache("Chrome");

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public void ScanBrowserCache_Edge_ReturnsNonNegativeValue()
    {
        // Act
        var result = CleanupUtilities.ScanBrowserCache("Edge");

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public void ScanBrowserCache_Firefox_ReturnsNonNegativeValue()
    {
        // Act
        var result = CleanupUtilities.ScanBrowserCache("Firefox");

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public void CleanBrowserCache_UnknownBrowser_ReturnsZero()
    {
        // Act
        var (cleaned, skipped) = CleanupUtilities.CleanBrowserCache("UnknownBrowser");

        // Assert
        Assert.Equal(0, cleaned);
        Assert.Equal(0, skipped);
    }
}
