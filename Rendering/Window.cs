using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System;

namespace Rendering
{
    public sealed class Window
    {
        private IWindow _window;
        private GL GL;

        public event Action<float> OnFrame;

        private Window(IWindow window, GL gl)
        {
            _window = window;
            GL = gl;
            _window.Render += Render;
        }

        public static Window CreateWindow(string windowTitle, System.Numerics.Vector2 size)
        {
            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>((int)size.X, (int)size.Y);
            options.Title = windowTitle;
            options.API = GraphicsAPI.Default;

            var window = Silk.NET.Windowing.Window.Create(options);

            GL gl = null;

            window.Load += () =>
            {
                gl = GL.GetApi(window);
                gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            };

            var _window = new Window(window, gl);

            window.Run();

            return _window;
        }

        internal GL GetGL()
        {
            return GL;
        }

        private void Render(double dt)
        {
            float actualDt = (float)dt;
            OnFrame?.Invoke(actualDt);
            renderMeshes();
        }

        private void renderMeshes()
        {
            if (GL == null) return;

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            foreach(var drawCall in MeshObjectHandler.GetMeshObjects())
            {
                drawCall.Material.Use();
                drawCall.Material.Render(GL, 0);

                GL.BindVertexArray((uint)drawCall.Mesh.vao);
                GL.DrawElements(PrimitiveType.Triangles, drawCall.Mesh.IndecesCount, DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}
