using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine.Rendering;

namespace RenderingPipeline
{
    public interface IRenderer<RT> : IDisposable where RT : RenderTask
    {
        string Name { get; set; }
        RT Task { get; }
        Task<RenderTarget> Output { get; } // Make sure to only create the outputs when the getter is actually called
    }
}
