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

        public CameraRenderer(Camera camera, Int2 size) : base(size)
        {
            Camera = camera;
        }

        public Camera Camera { get; }

        public override void Initialize()
        {
            Task.Camera = Camera;
            Task.Begin += OnRenderTaskInitialize;
            _output = GPUDevice.CreateTexture();
            var description = GPUTextureDescription.New2D(_size.X, _size.Y, PixelFormat.R8G8B8A8_UNorm);
            _output.Init(ref description);
            Task.Output = _output;
            _outputPromise.SetResult(_output);

            Task.Enabled = true;
        }

        private void OnRenderTaskInitialize(SceneRenderTask task, GPUContext context)
        {
            // TODO: Things like getting motion vectors is possible here
            // e.g. _depthBufferPromise.SetResult(task.Buffers.DepthBuffer);
            Task.Begin -= OnRenderTaskInitialize;
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
                        Task.Begin -= OnRenderTaskInitialize;
                    }
                    FlaxEngine.Object.Destroy(ref _output);
                }

                _disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
