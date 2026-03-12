using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EcommerceCatalog.Application.Interfaces;

namespace EcommerceCatalog.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(BlobServiceClient blobServiceClient, string containerName)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        // Ensure container exists
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        // Generate unique blob name to prevent overwrites
        var blobName = $"{Guid.NewGuid()}-{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
