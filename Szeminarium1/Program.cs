using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminárium - háromszög";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint lineVao = Gl.GenVertexArray();
            Gl.BindVertexArray(lineVao);

            float[] vertexArray = new float[] {
                0f, 0f, 0f, // A
                0f, -0.5f, 0f, // B
                0.4f,-0.2f, 0f, // C
                0.4f, 0.3f, 0f, // D

                // 4-7
                0f, 0f, 0f, // A
                -0.4f, 0.3f, 0f, // E
                -0.4f,-0.2f, 0f, // F
                0f, -0.5f, 0f, // B

                // 8-11
                0f, 0f, 0f, // A
                0.4f, 0.3f, 0f, // D
                0f, 0.5f, 0, // G
                -0.4f, 0.3f, 0f // E




                
                //-0.5f, -0.5f, 0.0f,
                //+0.5f, -0.5f, 0.0f,
                // 0.0f, +0.5f, 0.0f,
                // 1f, 1f, 0f
                // -0.8f, 0.8f, 0f
            }; // megadjuk a geometriáink adatait
               // 4 sor = 4 darab pont
               // leirja a poziciokat
               // normalizált eszkozkoordinatakban vannak: a koordinatak -1 es 1 kozottt
               // 1 koord: 3 db float: vizszintes és függőleges koordináták, illetve mélység (ezt első körben nem használjuk)


            float[] colorArray = new float[] {

                0.651f, 0.067f, 0.067f, 1.0f,
                0.651f, 0.067f, 0.067f, 1.0f,
                0.651f, 0.067f, 0.067f, 1.0f,
                0.651f, 0.067f, 0.067f, 1.0f,

                0.941f, 0.4f, 0.4f, 1.0f,
                0.941f, 0.4f, 0.4f, 1.0f,
                0.941f, 0.4f, 0.4f, 1.0f,
                0.941f, 0.4f, 0.4f, 1.0f,

                0.96f, 0.212f, 0.212f, 1.0f,
                0.96f, 0.212f, 0.212f, 1.0f,
                0.96f, 0.212f, 0.212f, 1.0f,
                0.96f, 0.212f, 0.212f, 1.0f,


            };

            uint[] indexArray = new uint[] {

                0, 1, 2,
                2, 3, 0,

                4, 5, 6,
                4, 6, 7,

                8,9, 10,
                8, 10, 11

                ////0, 1, 2
                //2, 1, 3
                ////0, 1, 3
            };
            // indexarray: a pontokat amit megkapott, azt rajzolásnál hogyan értelmezze?
            // megadunk egy rajzolasi modot (esetunkben haromszogek), 

            float[] lineVertices = new float[]
            {
                // Függőleges vonalak
                0.13f, -0.4f, 0f,  // Kezdőpont
                0.13f, 0.1f, 0f,   // Végpont
                0.26f, -0.3f, 0f,  // Kezdőpont
                0.26f, 0.2f, 0f,   // Végpont
                -0.13f, -0.4f, 0f,  // Kezdőpont
                -0.13f, 0.1f, 0f,   // Végpont
                -0.26f, -0.3f, 0f,  // Kezdőpont
                -0.26f, 0.2f, 0f,   // Végpont

                // Vizszintes vonalak
                0f, -0.33f, 0f,  // Kezdőpont
                0.4f, -0.033f, 0f,   // Végpont
                0f, -0.16f, 0f,  // Kezdőpont
                0.4f, 0.133f, 0f,   // Végpont
                0f, -0.33f, 0f,  // Kezdőpont
                -0.4f, -0.033f, 0f,   // Végpont
                0f, -0.16f, 0f,  // Kezdőpont
                -0.4f, 0.133f, 0f,   // Végpont

                // Kocka tetején 1
                0.13f, 0.1f, 0f,
                -0.26f, 0.36f, 0f,
                0.26f, 0.2f, 0f,
                -0.13f, 0.43f, 0f,

                // Kocka tetején 2
                -0.13f, 0.1f, 0f,
                0.26f, 0.36f, 0f,
                -0.26f, 0.2f, 0f,
                0.13f, 0.43f, 0f,


            };

            uint[] lineIndices = new uint[]
            {
                0, 1,
                2, 3,
                4, 5,
                6, 7,
                8, 9,
                10, 11,
                12, 13,
                14, 15,
                16, 17,
                18, 19,
                20, 21,
                22, 23,
                //24, 25,
                //26, 27,
            };



            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            // vonalak rajzolása:
            uint lineVbo = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineVbo);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)lineVertices.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint lineEbo = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, lineEbo);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)lineIndices.AsSpan(), GLEnum.StaticDraw);

            Gl.UseProgram(program);

            // Ez itt fontos:
            Gl.LineWidth(3.0f);
            Gl.DrawElements(GLEnum.Lines, (uint)lineIndices.Length, GLEnum.UnsignedInt, null);



            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);


            // unboundolás: vonalak
            Gl.DeleteBuffer(lineVbo);
            Gl.DeleteBuffer(lineEbo);
            Gl.DeleteVertexArray(lineVao);

        }
    }
}
