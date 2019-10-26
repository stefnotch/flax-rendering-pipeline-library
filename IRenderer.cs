using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.Rendering;

namespace RenderingPipeline
{
    public interface IRenderer : IDisposable
    {
        string Name { get; set; }
        Task<RenderTarget> Output { get; }
    }
}
