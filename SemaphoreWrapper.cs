namespace SX1268Library;

public class SemaphoreLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private bool _taken;

    internal SemaphoreLock(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
        _taken = true;
    }

    public void Dispose()
    {
        if (_taken)
        {
            _semaphore.Release();
            _taken = false;
        }
    }
}

/// <summary>
/// Wrapper class for semaphore/synchronization operations.
/// </summary>
public class SemaphoreWrapper : IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public SemaphoreWrapper(int initial, int max)
    {
        _semaphore = new SemaphoreSlim(initial, max);
    }

    public SemaphoreWrapper(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }

    public SemaphoreLock LockOne(TimeSpan? timeout = null)
    {
        if (timeout.HasValue)
        {
            if (!_semaphore.Wait(timeout.Value))
            {
                throw new TimeoutException("Semaphore operation has timed out.");
            }
        }
        else
        {
            _semaphore.Wait();
        }

        return new SemaphoreLock(_semaphore);
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}