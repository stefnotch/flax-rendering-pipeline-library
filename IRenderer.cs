using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace RenderingPipeline
{
    public interface IRenderer : IDisposable
    {
        string Name { get; set; }
        Task<GPUTexture> Output { get; }
    }
}
