using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace RenderingPipeline
{

    public class RendererPipeline : IDisposable
    {
        private readonly List<IRenderer> _renderers = new List<IRenderer>();
        private bool _disposedValue = false;

        // TODO: The size can be adjusted at runtime by setting the IRenderer.Output size. Document this & use it.
        // TODO: Somehow set the task order.

        public T AddRenderer<T>(T renderer) where T : IRenderer
        {
            _renderers.Add(renderer);
            return renderer;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    for (int i = 0; i < _renderers.Count; i++)
                    {
                        _renderers[i].Dispose();
                    }
                    _renderers.Clear();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
