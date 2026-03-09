using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.StoredFiles;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace JcStack.Abp.BlobStorage;

public class BlobStorageTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IStoredFileRepository _storedFileRepository;

    public BlobStorageTestDataSeedContributor(
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IStoredFileRepository storedFileRepository)
    {
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _storedFileRepository = storedFileRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await SeedStoredFilesAsync();
        }
    }

    private async Task SeedStoredFilesAsync()
    {
        // StoredFile 1 - PDF 文档
        if (await _storedFileRepository.FindAsync(BlobStorageTestConsts.StoredFile1Id) == null)
        {
            await _storedFileRepository.InsertAsync(
                new StoredFile(
                    BlobStorageTestConsts.StoredFile1Id,
                    BlobStorageTestConsts.StoredFile1FileName,
                    BlobStorageTestConsts.StoredFile1ContentType,
                    BlobStorageTestConsts.StoredFile1FileSize,
                    BlobStorageTestConsts.DefaultBlobContainerName,
                    BlobStorageTestConsts.StoredFile1BlobName,
                    BlobStorageTestConsts.StoredFile1Checksum),
                autoSave: true);
        }

        // StoredFile 2 - 图片
        if (await _storedFileRepository.FindAsync(BlobStorageTestConsts.StoredFile2Id) == null)
        {
            await _storedFileRepository.InsertAsync(
                new StoredFile(
                    BlobStorageTestConsts.StoredFile2Id,
                    BlobStorageTestConsts.StoredFile2FileName,
                    BlobStorageTestConsts.StoredFile2ContentType,
                    BlobStorageTestConsts.StoredFile2FileSize,
                    BlobStorageTestConsts.DefaultBlobContainerName,
                    BlobStorageTestConsts.StoredFile2BlobName),
                autoSave: true);
        }

        // StoredFile 3 - Excel
        if (await _storedFileRepository.FindAsync(BlobStorageTestConsts.StoredFile3Id) == null)
        {
            await _storedFileRepository.InsertAsync(
                new StoredFile(
                    BlobStorageTestConsts.StoredFile3Id,
                    BlobStorageTestConsts.StoredFile3FileName,
                    BlobStorageTestConsts.StoredFile3ContentType,
                    BlobStorageTestConsts.StoredFile3FileSize,
                    BlobStorageTestConsts.DefaultBlobContainerName,
                    BlobStorageTestConsts.StoredFile3BlobName),
                autoSave: true);
        }
    }
}
