using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace RenderingPipeline
{
    public abstract class GenericRenderer<RT> : IRenderer where RT : RenderTask
    {
        private bool _disposedValue = false;

        protected TaskCompletionSource<GPUTexture> _outputPromise;

        protected Int2 _cachedSize;
        public Func<Int2> SizeGetter { get; set; }

        protected GenericRenderer(Func<Int2> sizeGetter)
        {
            Task = FlaxEngine.Object.New<RT>();
            Task.Enabled = false;
            SizeGetter = sizeGetter;
        }

        public string Name { get; set; }

        public RT Task { get; protected set; }

        public Task<GPUTexture> Output
        {
            get
            {
                if (_outputPromise == null)
                {
                    _outputPromise = new TaskCompletionSource<GPUTexture>();
                    Initialize();
                }

                return _outputPromise.Task;
            }
        }

        public abstract void Initialize();

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _outputPromise?.TrySetCanceled();
                    if (Task)
                    {
                        Task.Enabled = false;
                        Task.Dispose();
                    }
                    FlaxEngine.Object.Destroy(Task);
                    Task = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
