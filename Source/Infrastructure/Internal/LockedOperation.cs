namespace AutomaticDotNETtrading.Infrastructure.Internal;

internal class LockedOperation : IDisposable
{
    private readonly SemaphoreSlim Semaphore;
    public LockedOperation(SemaphoreSlim semaphore)
    {
        this.Semaphore = semaphore;
        this.Semaphore.Wait();
    }

    public void Dispose()
    {
        this.Semaphore.Release();
    }
}
