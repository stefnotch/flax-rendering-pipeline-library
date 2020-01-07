using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace RenderingPipeline
{
    public class PostFxRenderer : GenericRenderer<CustomRenderTask>
    {
        private bool _disposedValue = false;
        private GPUTexture _output;

        public PostFxRenderer(MaterialBase material, Func<Int2> sizeGetter) : base(sizeGetter)
        {
            if (!material.IsPostFx) throw new ArgumentException("PostFx Material expected", nameof(material));
            if (!material) throw new ArgumentNullException(nameof(material));

            Material = material;
        }

        public MaterialBase Material { get; private set; }

        /// <summary>
        /// The <see cref="RenderTarget"/> that gets post-processed
        /// </summary>
        public GPUTexture Input { get; private set; }

        public override void Initialize()
        {
            _output = GPUDevice.CreateTexture();
            _cachedSize = SizeGetter();
            var description = GPUTextureDescription.New2D(_cachedSize.X, _cachedSize.Y, PixelFormat.R8G8B8A8_UNorm);
            _output.Init(ref description);
            _outputPromise.SetResult(_output);

            Task.Render = OnRender;
            Task.Enabled = true;
        }

        public PostFxRenderer SetInput(Task<GPUTexture> renderTargetTask)
        {
            renderTargetTask.ContinueWith((task) => SetInput(task.Result), TaskContinuationOptions.NotOnCanceled);
            return this;
        }

        public PostFxRenderer SetInput(GPUTexture texture)
        {
            Input = texture;
            return this;
        }

        public PostFxRenderer SetInput(string name, Task<GPUTexture> renderTargetTask)
        {
            renderTargetTask.ContinueWith((task) => SetInput(name, task.Result), TaskContinuationOptions.NotOnCanceled);
            return this;
        }

        public PostFxRenderer SetInput(string name, GPUTexture texture)
        {
            var param = Material.GetParam(name);
            if (param != null)
            {
                param.Value = texture;
            }
            return this;
        }

        private void OnRender(GPUContext context)
        {
            Int2 size = SizeGetter();
            if (_cachedSize != size)
            {
                _output.Size = new Vector2(size.X, size.Y);
                _cachedSize = size;
            }
            context.DrawPostFxMaterial(Material, _output, Input);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    FlaxEngine.Object.Destroy(ref _output);
                    Material = null;
                }

                _disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
