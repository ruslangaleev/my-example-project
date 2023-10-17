using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using MyExampleProject.S3.Configurations;

namespace MyExampleProject.ApiControllers
{
    /// <summary>
    /// Взаимодействие с облачным хранилищем через S3, напримере Selectel.
    /// </summary>
    [Route("api/s3")]
    [ApiController]
    public class S3Controller : ControllerBase
    {
        private readonly bool _isActive;
        private readonly IMinioClient _minioClient;
        private readonly string _bucket;

        public S3Controller(IOptions<FileStorageByS3Config> options)
        {
            var minioClient = new MinioClient()
                .WithEndpoint("s3.storage.selcloud.ru")
                .WithCredentials(accessKey: options.Value.AccessKey, secretKey: options.Value.SecretKey)
                .WithSSL(secure: true)
                .WithRegion("ru-1")
                .Build();

            _minioClient = minioClient;
            _isActive = options.Value.IsActive;
            _bucket = options.Value.Bucket;
        }

        /// <summary>
        /// Скачивает файл с хранилища и предоставит содержимое файла.
        /// </summary>
        /// <param name="pathToFile">Важно чтобы путь был через косую черту /. Например: dir1/dir2</param>
        /// <param name="fileName">Имя файла, который будет скачен.</param>
        /// <returns></returns>
        [HttpGet("{pathToFile}/{fileName}")]
        public async Task<IActionResult> GetFileAsync(string pathToFile, string fileName)
        {
            if (!_isActive)
            {
                return BadRequest("Is not active");
            }

            var fullPathToFile = $"{pathToFile}/{fileName}";

            using (MemoryStream memoryStream = new MemoryStream())
            {
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(fullPathToFile)
                    .WithCallbackStream((stream) => { stream.CopyTo(memoryStream); });

                try
                {
                    var response = await _minioClient.GetObjectAsync(getObjectArgs).ConfigureAwait(false);
                }
                catch (ObjectNotFoundException)
                {
                    return BadRequest("File not found");
                }
                catch (Exception)
                {
                    return BadRequest("Error find file");
                }

                return File(memoryStream.ToArray(), contentType: "text/plain");
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetObjectsOfBucketAsync()
        {
            if (!_isActive)
            {
                return BadRequest("Is not active");
            }

            var listArgs = new ListObjectsArgs()
                .WithBucket(_bucket);

            var items = new List<Item>();

            var observable = _minioClient.ListObjectsAsync(listArgs);

            var isCompleted = false;

            while(!isCompleted)
            {
                observable.Subscribe(
                    item => 
                    {
                        items.Add(item);
                    },
                    ex => { },
                    () => 
                    {
                        isCompleted = true;
                    });
            }

            return Ok(items);
        }

        public class FileUploadRequest
        {
            public string PathToFile { get; set; }

            public string FileName { get; set; }

            public IFormFile File { get; set; }
        }

        /// <summary>
        /// Если указать имя файла который уже существует в хранилище, то он перезапишет его.
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("{pathToFile}/{fileName}")]
        public async Task<IActionResult> SaveFileAsync(string pathToFile, string fileName, IFormFile file)
        {
            if (!_isActive)
            {
                return BadRequest("Is not active");
            }

            var fullPathToFile = $"{pathToFile}/{fileName}";

            var args = new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(fullPathToFile)
                .WithObjectSize(file.Length)
                .WithStreamData(file.OpenReadStream());

            var response = await _minioClient.PutObjectAsync(args).ConfigureAwait(false);

            return Ok();
        }

        [HttpDelete("{pathToFile}/{fileName}")]
        public async Task<IActionResult> DeleteFileAsync(string pathToFile, string fileName)
        {
            if (!_isActive)
            {
                return BadRequest("Is not active");
            }

            var fullPathToFile = $"{pathToFile}/{fileName}";

            var args = new RemoveObjectArgs()
                .WithBucket(_bucket)
                .WithObject(fullPathToFile);

            await _minioClient.RemoveObjectAsync(args).ConfigureAwait(false);

            return Ok();
        }
    }
}
