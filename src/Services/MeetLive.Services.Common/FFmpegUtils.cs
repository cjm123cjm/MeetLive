using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class FFmpegUtils
{
    private readonly ILogger<FFmpegUtils> _logger;
    public FFmpegUtils(ILogger<FFmpegUtils> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// 转换图片格式（通常用于格式标准化）
    /// </summary>
    /// <param name="tempFile">临时文件</param>
    /// <param name="filePath">目标文件路径</param>
    /// <returns>转换后的文件路径</returns>
    public string TransferImageType(FileInfo tempFile, string filePath)
    {
        const string CMD_CREATE_IMAGE_THUMBNAIL = "ffmpeg -i \"{0}\" \"{1}\"";
        string cmd = string.Format(CMD_CREATE_IMAGE_THUMBNAIL, tempFile.FullName, filePath);

        ExecuteCommand(cmd);

        // 删除临时文件
        tempFile.Delete();

        return filePath;
    }

    /// <summary>
    /// 转换视频格式（HEVC/H.265转MP4）
    /// </summary>
    /// <param name="tempFile">临时文件</param>
    /// <param name="filePath">目标文件路径</param>
    /// <param name="fileSuffix">文件后缀</param>
    public async Task TransferVideoTypeAsync(FileInfo tempFile, string filePath, string fileSuffix)
    {
        string videoCodec = await GetVideoCodecAsync(tempFile.FullName);

        // 如果是HEVC编码或某些特定后缀，需要转换
        if (Constants.VIDEO_CODE_HEVC.Equals(videoCodec, StringComparison.OrdinalIgnoreCase) ||
            Constants.VIDEO_SUFFIX.Equals(fileSuffix, StringComparison.OrdinalIgnoreCase))
        {
            await ConvertHevcToMp4Async(tempFile.FullName, filePath);
        }
        else
        {
            // 直接复制文件
            File.Copy(tempFile.FullName, filePath, true);
        }

        // 删除临时文件
        tempFile.Delete();
    }

    /// <summary>
    /// 生成视频缩略图（异步版本）
    /// </summary>
    public async Task<string> CreateVideoThumbnailAsync(string videoFilePath, string thumbnailPath = null,
        string timePosition = "00:00:01", int scaleWidth = 200)
    {
        try
        {
            if (string.IsNullOrEmpty(thumbnailPath))
            {
                thumbnailPath = GetVideoThumbnailPath(videoFilePath);
            }

            const string CMD_CREATE_VIDEO_THUMBNAIL = "ffmpeg -i \"{0}\" -ss {1} -vframes 1 -vf scale={2}:-1 \"{3}\" -y";
            string cmd = string.Format(CMD_CREATE_VIDEO_THUMBNAIL,
                videoFilePath, timePosition, scaleWidth, thumbnailPath);

            await ExecuteCommandAsync(cmd);

            // 验证文件
            if (File.Exists(thumbnailPath) && new FileInfo(thumbnailPath).Length > 0)
            {
                return thumbnailPath;
            }

            throw new InvalidOperationException("视频缩略图生成失败");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "生成视频缩略图失败");
            return await CreateVideoThumbnailFallbackAsync(videoFilePath, thumbnailPath);
        }
    }

    /// <summary>
    /// 生成视频缩略图（备用方案 - 异步）
    /// </summary>
    private async Task<string> CreateVideoThumbnailFallbackAsync(string videoFilePath, string thumbnailPath)
    {
        try
        {
            if (string.IsNullOrEmpty(thumbnailPath))
            {
                thumbnailPath = GetVideoThumbnailPath(videoFilePath);
            }

            string[] timePositions = { "00:00:01", "00:00:03", "00:00:05", "00:00:10" };

            foreach (var timePos in timePositions)
            {
                try
                {
                    const string CMD_CREATE_VIDEO_THUMBNAIL_FALLBACK = "ffmpeg -i \"{0}\" -ss {1} -vframes 1 \"{2}\" -y";
                    string cmd = string.Format(CMD_CREATE_VIDEO_THUMBNAIL_FALLBACK,
                        videoFilePath, timePos, thumbnailPath);

                    await ExecuteCommandAsync(cmd);

                    if (File.Exists(thumbnailPath) && new FileInfo(thumbnailPath).Length > 0)
                    {
                        return thumbnailPath;
                    }
                }
                catch
                {
                    // 继续尝试
                }
            }

            // 创建默认缩略图
            await CreateDefaultThumbnailAsync(thumbnailPath);
            return thumbnailPath;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "视频缩略图备用方案失败");
            return thumbnailPath;
        }
    }

    /// <summary>
    /// 获取视频缩略图路径
    /// </summary>
    private string GetVideoThumbnailPath(string videoFilePath)
    {
        string directory = Path.GetDirectoryName(videoFilePath);
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(videoFilePath);

        // 视频缩略图使用.jpg格式
        return Path.Combine(directory, $"{fileNameWithoutExt}_thumb.jpg");
    }

    /// <summary>
    /// 获取视频编码格式
    /// </summary>
    private async Task<string> GetVideoCodecAsync(string filePath)
    {
        const string CMD_GET_VIDEO_CODEC = "ffprobe -v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"{0}\"";
        string cmd = string.Format(CMD_GET_VIDEO_CODEC, filePath);

        string output = await ExecuteCommandAsync(cmd);
        return output?.Trim();
    }

    /// <summary>
    /// 将HEVC/H.265视频转换为MP4格式
    /// </summary>
    private async Task ConvertHevcToMp4Async(string inputPath, string outputPath)
    {
        // 转换为H.264编码的MP4，兼容性更好
        const string CMD_CONVERT_HEVC_TO_MP4 = "ffmpeg -i \"{0}\" -c:v libx264 -c:a aac -movflags +faststart \"{1}\" -y";
        string cmd = string.Format(CMD_CONVERT_HEVC_TO_MP4, inputPath, outputPath);

        await ExecuteCommandAsync(cmd);
    }

    /// <summary>
    /// 生成图片缩略图（从文件路径）
    /// </summary>
    public void CreateImageThumbnail(string filePath)
    {
        // 生成缩略图文件名（添加_thumb后缀）
        string thumbnailPath = GetImageThumbnailPath(filePath);

        const string CMD_CREATE_IMAGE_THUMBNAIL = "ffmpeg -i \"{0}\" -vf scale=200:-1 \"{1}\" -y";
        string cmd = string.Format(CMD_CREATE_IMAGE_THUMBNAIL, filePath, thumbnailPath);

        ExecuteCommand(cmd);
    }

    /// <summary>
    /// 生成图片缩略图（从临时文件）
    /// </summary>
    public void CreateImageThumbnail(FileInfo tempFile, string filePath)
    {
        const string CMD_CREATE_IMAGE_THUMBNAIL = "ffmpeg -i \"{0}\" -vf scale=200:-1 \"{1}\" -y";
        string cmd = string.Format(CMD_CREATE_IMAGE_THUMBNAIL, tempFile.FullName, filePath);

        ExecuteCommand(cmd);
        tempFile.Delete();
    }

    /// <summary>
    /// 获取缩略图文件路径
    /// </summary>
    private string GetImageThumbnailPath(string originalPath)
    {
        string directory = Path.GetDirectoryName(originalPath);
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
        string extension = Path.GetExtension(originalPath);

        // 在文件名后添加_thumb后缀
        return Path.Combine(directory, $"{fileNameWithoutExt}_thumb{extension}");
    }

    /// <summary>
    /// 同步执行命令
    /// </summary>
    private void ExecuteCommand(string command)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"命令执行失败: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"执行命令时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步执行命令
    /// </summary>
    private async Task<string> ExecuteCommandAsync(string command)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"命令执行失败: {error}");
                }

                return output;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"执行命令时发生错误: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// 常量定义
/// </summary>
public static class Constants
{
    /// <summary>
    /// HEVC/H.265视频编码标识
    /// </summary>
    public const string VIDEO_CODE_HEVC = "hevc";

    /// <summary>
    /// 需要转换的视频后缀
    /// </summary>
    public const string VIDEO_SUFFIX = ".hevc";
}

/// <summary>
/// 字符串工具类
/// </summary>
public static class StringTools
{
    /// <summary>
    /// 获取图片缩略图路径（扩展方法）
    /// </summary>
    public static string GetImageThumbnail(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        return Path.Combine(directory, $"{fileNameWithoutExt}_thumb{extension}");
    }
}

/// <summary>
/// 文件工具类
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// 复制文件
    /// </summary>
    public static void CopyFile(string sourcePath, string destPath)
    {
        File.Copy(sourcePath, destPath, true);
    }
}