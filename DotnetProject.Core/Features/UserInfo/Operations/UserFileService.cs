using DotnetProject.Core.Shared;
using FeedCommonLib.Application.Abstractions.EndpointExtension;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace DotnetProject.Core.Features.UserInfo.Operations
{
    public static class UserFileService
    {
        public static Task<Result<FileInfoRecord>> DownloadUserFile(string FileName, ILogger logger)
        {
            // Assume you have a file stream and metadata
            //D:\Project\DotnetProject\src\DotnetProject.Api\Temp\rambo.jpg
            try
            {
                string path = $"D:/Project/DotnetProject/src/DotnetProject.Api/Temp/{FileName}";
                if (File.Exists(path))
                {
                    var fileStream = File.OpenRead(path);
                    // Get content type by file extension
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(FileName, out var _contentType))
                    {
                        _contentType = "application/octet-stream"; // Default fallback
                    }
                    FileInfoRecord fInfo = new FileInfoRecord()
                    {
                        FileStream = fileStream,
                        FileName = FileName,
                        ContentType = _contentType
                    };

                    // Wrap in Result<FileInfoRecord>
                    return Task.FromResult(Result<FileInfoRecord>.Success(fInfo, SuccessCodes.DownloadCompleted));
                }

                StdResponse error = StdResponse.Create(Errors.NotFound, data: FileName);
                return Task.FromResult(Result<FileInfoRecord>.Failure(error));
            }
            catch (Exception)
            {
                StdResponse error = StdResponse.Create(Errors.Unexpected, data: FileName);
                return Task.FromResult(Result<FileInfoRecord>.Failure(error));
            }
            

        }

        public static  Result<ApiStreamResponse> CreateFileInfo(FileInfoRecord fInfo)
        {
            try
            {
                var streamResponse = ApiStreamResponse.Success(
                   stream: fInfo.FileStream!,
                   fileName: fInfo.FileName!,
                   contentType: fInfo.ContentType
               );

                return Result<ApiStreamResponse>.Success(streamResponse, SuccessCodes.DownloadCompleted);
            }
            catch (Exception)
            {
                StdResponse error = StdResponse.Create(Errors.Unexpected, data: fInfo);
                return  Result<ApiStreamResponse>.Failure(error); 
            } 
        }
    }
}
