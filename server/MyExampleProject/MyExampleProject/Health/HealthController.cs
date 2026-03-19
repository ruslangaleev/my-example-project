using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyExampleProject.S3.Configurations;

namespace MyExampleProject.Health
{
    /// <summary>
    /// Проверка доступности сервиса и статуса подключения к хранилищу.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly FileStorageByS3Config _storageConfig;

        public HealthController(IOptions<FileStorageByS3Config> storageConfig)
        {
            _storageConfig = storageConfig.Value;
        }

        /// <summary>
        /// Возвращает статус сервиса и признак настройки облачного хранилища.
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "ok",
                storageConfigured = _storageConfig.IsActive && !string.IsNullOrWhiteSpace(_storageConfig.Bucket)
            });
        }
    }
}
