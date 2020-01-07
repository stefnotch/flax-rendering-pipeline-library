using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace RenderingPipeline
{
    public class CameraRenderer : GenericRenderer<SceneRenderTask>
    {
        private bool _disposedValue = false;
        private GPUTexture _output;

        public CameraRenderer(Camera camera, Func<Int2> sizeGetter) : base(sizeGetter)
        {
            Camera = camera;
        }

        public Camera Camera { get; }

        public override void Initialize()
        {
            Task.Camera = Camera;
            Task.Begin += OnBegin;
            _output = GPUDevice.CreateTexture();
            _cachedSize = SizeGetter();
            var description = GPUTextureDescription.New2D(_cachedSize.X, _cachedSize.Y, PixelFormat.R8G8B8A8_UNorm);
            _output.Init(ref description);
            Task.Output = _output;
            _outputPromise.SetResult(_output);

            Task.Enabled = true;
        }

        private void OnBegin(SceneRenderTask task, GPUContext context)
        {
            // TODO: Things like getting motion vectors is possible here
            // e.g. _depthBufferPromise.SetResult(task.Buffers.DepthBuffer);
            Int2 size = SizeGetter();
            if (_cachedSize != size)
            {
                _output.Size = new Vector2(size.X, size.Y);
                _cachedSize = size;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Task)
                    {
                        // Custom RenderTask disposal code
                        Task.Begin -= OnBegin;
                    }
                    FlaxEngine.Object.Destroy(ref _output);
                }

                _disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
