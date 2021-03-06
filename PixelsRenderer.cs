using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace RenderingPipeline
{
    /// <summary>
    /// Used for effects where individual pixels need to be moved.
    /// </summary>
    public class PixelsRenderer : GenericRenderer<SceneRenderTask>
    {
        private readonly float DistanceFromOrigin = 100f;
        private bool _disposedValue = false;
        private GPUTexture _output;
        private Camera _orthographicCamera;
        private StaticModel _modelActor;
        private Model _model;

        public PixelsRenderer(MaterialBase material, Func<Int2> sizeGetter) : base(sizeGetter)
        {
            if (!material.IsSurface) throw new ArgumentException("Surface Material expected", nameof(material));
            if (!material) throw new ArgumentNullException(nameof(material));

            Material = material;
        }

        public MaterialBase Material { get; private set; }

        public override void Initialize()
        {
            _cachedSize = SizeGetter();

            _orthographicCamera = CreateOrthographicCamera();
            Task.Camera = _orthographicCamera;
            _model = Content.CreateVirtualAsset<Model>();
            _model.SetupLODs(1);
            _model.SetupMaterialSlots(1);
            _model.MaterialSlots[0].ShadowsMode = ShadowsCastingMode.None;
            // TODO: Optimize, use instanced rendering and whatnot
            GenerateGridMesh(_model.LODs[0].Meshes[0], _cachedSize);
            _modelActor = FlaxEngine.Object.New<StaticModel>();
            _modelActor.Model = _model;
            _modelActor.Entries[0].ReceiveDecals = false;
            _modelActor.Entries[0].ShadowsMode = ShadowsCastingMode.None;
            _modelActor.LocalPosition = new Vector3(new Vector2(_cachedSize.X, _cachedSize.Y) * -0.5f, DistanceFromOrigin);
            _modelActor.Entries[0].Material = Material;

            Task.CustomActors.Add(_modelActor);
            Task.AllowGlobalCustomPostFx = false;
            Task.ActorsSource = ActorsSources.CustomActors;
            Task.View.Mode = ViewMode.Emissive;
            Task.View.Flags = ViewFlags.None;
            Task.Begin += OnRenderTaskBegin;
            _output = GPUDevice.CreateTexture();
            var description = GPUTextureDescription.New2D(_cachedSize.X, _cachedSize.Y, PixelFormat.R8G8B8A8_UNorm);
            _output.Init(ref description);
            Task.Output = _output;
            _outputPromise.SetResult(_output);

            Task.Enabled = true;
        }

        public PixelsRenderer SetInput(string name, Task<GPUTexture> renderTargetTask)
        {
            renderTargetTask.ContinueWith((task) => SetInput(name, task.Result), TaskContinuationOptions.NotOnCanceled);
            return this;
        }

        public PixelsRenderer SetInput(string name, GPUTexture texture)
        {
            var param = Material.GetParam(name);
            if (param != null)
            {
                param.Value = texture;
            }
            return this;
        }

        private void OnRenderTaskBegin(SceneRenderTask task, GPUContext context)
        {
            Int2 size = SizeGetter();
            if (_cachedSize != size)
            {
                _output.Size = new Vector2(size.X, size.Y);
                _modelActor.LocalPosition = new Vector3(size.X * -0.5f, size.Y * -0.5f, DistanceFromOrigin);
                _cachedSize = size;
            }
        }

        private static Camera CreateOrthographicCamera()
        {
            var orthographicCamera = FlaxEngine.Object.New<Camera>();
            orthographicCamera.NearPlane = 2;
            orthographicCamera.FarPlane = 1000;
            orthographicCamera.OrthographicScale = 1;
            orthographicCamera.LocalPosition = Vector3.Zero;
            orthographicCamera.UsePerspective = false;
            return orthographicCamera;
        }

        private static void GenerateGridMesh(Mesh mesh, Int2 size)
        {
            if (mesh == null) return;

            int width = size.X;
            int height = size.Y;

            bool UVHalfPixelOffset = false; // TODO: Do I need this?
            Vector3[] vertices = new Vector3[width * height * 6];
            Vector2[] uvs = new Vector2[width * height * 6];
            int[] triangles = new int[width * height * 6];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = (y + x * height) * 6;

                    vertices[index] = new Vector3(x, y, 0);
                    vertices[index + 1] = new Vector3(x, y + 1, 0);
                    vertices[index + 2] = new Vector3(x + 1, y, 0);

                    vertices[index + 3] = new Vector3(x + 1, y, 0);
                    vertices[index + 4] = new Vector3(x, y + 1, 0);
                    vertices[index + 5] = new Vector3(x + 1, y + 1, 0);

                    Vector2 uv;
                    if (UVHalfPixelOffset)
                    {
                        uv = new Vector2(
                                (x + 0.5f) / (float)width,
                                1f - (y + 0.5f) / (float)height
                            );
                    }
                    else
                    {
                        uv = new Vector2(
                                x / (float)width,
                                1f - y / (float)height
                            );
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        uvs[index + i] = uv;
                    }
                }
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = i;
            }

            mesh.UpdateMesh(vertices, triangles, uv: uvs);
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
                    FlaxEngine.Object.Destroy(ref _orthographicCamera);
                    FlaxEngine.Object.Destroy(ref _modelActor);
                    FlaxEngine.Object.Destroy(ref _model);
                    Material = null;
                }

                _disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
