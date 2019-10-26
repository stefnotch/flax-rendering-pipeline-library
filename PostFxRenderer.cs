using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.Rendering;

namespace RenderingPipeline
{
    public class PostFxRenderer : GenericRenderer<CustomRenderTask>
    {
        private bool _disposedValue = false;
        private RenderTarget _output;

        public PostFxRenderer(MaterialBase material, Int2 size) : base(size)
        {
            if (!material.IsPostFx) throw new ArgumentException("PostFx Material expected", nameof(material));
            if (!material) throw new ArgumentNullException(nameof(material));

            Material = material;
        }

        public MaterialBase Material { get; private set; }

        /// <summary>
        /// The <see cref="RenderTarget"/> that gets post-processed
        /// </summary>
        public RenderTarget Input { get; private set; }

        public override void Initialize()
        {
            _output = RenderTarget.New();
            _output.Init(PixelFormat.R8G8B8A8_UNorm, _size.X, _size.Y);
            _outputPromise.SetResult(_output);

            Task.Render = OnRender;
            Task.Enabled = true;
        }

        public PostFxRenderer SetInput(Task<RenderTarget> renderTargetTask)
        {
            renderTargetTask.ContinueWith((task) => SetInput(task.Result), TaskContinuationOptions.NotOnCanceled);
            return this;
        }

        public PostFxRenderer SetInput(RenderTarget renderTarget)
        {
            Input = renderTarget;
            return this;
        }

        public PostFxRenderer SetInput(string name, Task<RenderTarget> renderTargetTask)
        {
            renderTargetTask.ContinueWith((task) => SetInput(name, task.Result), TaskContinuationOptions.NotOnCanceled);
            return this;
        }

        public PostFxRenderer SetInput(string name, RenderTarget renderTarget)
        {
            var param = Material.GetParam(name);
            if (param != null)
            {
                param.Value = renderTarget;
            }
            return this;
        }

        private void OnRender(GPUContext context)
        {
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
