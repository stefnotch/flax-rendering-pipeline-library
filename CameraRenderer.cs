using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.Rendering;

namespace RenderingPipeline
{
    public class CameraRenderer : AbstractRenderer<SceneRenderTask>
    {
        private bool _disposedValue = false;
        private RenderTarget _output;

        public CameraRenderer(Camera camera, Int2 size)
        {
            Camera = camera;
            Size = size;
        }

        public Camera Camera { get; }
        public Int2 Size { get; }

        public override void Initialize()
        {
            Task.Camera = Camera;
            Task.Begin += OnRenderTaskBegin;
            _output = RenderTarget.New();
            _output.Init(PixelFormat.R8G8B8A8_UNorm, Size.X, Size.Y);
            Task.Output = _output;
            _outputPromise.SetResult(_output);

            Task.Enabled = true;
        }

        private void OnRenderTaskBegin(SceneRenderTask task, GPUContext context)
        {
            // TODO: Things like getting motion vectors is possible here
            // e.g. _depthBufferPromise.SetResult(task.Buffers.DepthBuffer);
            Task.Begin -= OnRenderTaskBegin;
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
                        Task.Begin -= OnRenderTaskBegin;
                    }
                    FlaxEngine.Object.Destroy(ref _output);
                }

                _disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
