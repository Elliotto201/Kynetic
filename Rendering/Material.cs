using Silk.NET.OpenGL;

namespace Rendering
{
    public abstract class Material
    {
        private GL _gl;
        private uint shaderId;

        internal Material(GL gl, string vertSource, string fragSource)
        {
            _gl = gl;

            uint vertex = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertex, vertSource);
            _gl.CompileShader(vertex);

            int status;
            _gl.GetShader(vertex, (GLEnum)ShaderParameterName.CompileStatus, out status);
            if (status == 0)
            {
                string log = _gl.GetShaderInfoLog(vertex);
                throw new Exception("Vertex shader compilation failed: " + log);
            }

            uint fragment = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragment, fragSource);
            _gl.CompileShader(fragment);

            _gl.GetShader(fragment, (GLEnum)ShaderParameterName.CompileStatus, out status);
            if (status == 0)
            {
                string log = _gl.GetShaderInfoLog(fragment);
                throw new Exception("Fragment shader compilation failed: " + log);
            }

            shaderId = _gl.CreateProgram();
            _gl.AttachShader(shaderId, vertex);
            _gl.AttachShader(shaderId, fragment);
            _gl.LinkProgram(shaderId);

            _gl.GetProgram(shaderId, (GLEnum)ProgramPropertyARB.LinkStatus, out status);
            if (status == 0)
            {
                string log = _gl.GetProgramInfoLog(shaderId);
                throw new Exception("Shader program linking failed: " + log);
            }

            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
        }

        internal static T CreateMaterial<T>(GL gl, string vertSource, string fragSource) where T : Material, new()
        {
            return (T)Activator.CreateInstance(typeof(T), gl, vertSource, fragSource);
        }

        internal void Use()
        {
            _gl.UseProgram(shaderId);
        }

        /// <summary>
        /// A method used to set things like uniforms
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="dt"></param>
        public abstract void Render(GL gl, float dt);
    }
}
