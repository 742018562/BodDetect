using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BodDetect
{
    sealed class AsyncSemaphore : IDisposable

    {

        private class SemaphoreRelaese : IDisposable

        {

            private SemaphoreSlim _semaphore;

            public SemaphoreRelaese(SemaphoreSlim semaphore) => _semaphore = semaphore;

            public void Dispose()

            {

                _semaphore.Release();

            }

        }

        private SemaphoreSlim _semaphore;

        public AsyncSemaphore() => _semaphore = new SemaphoreSlim(1);

        public async Task<IDisposable> WaitAsync()

        {

            await _semaphore.WaitAsync();

            return new SemaphoreRelaese(_semaphore);

        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }

}
