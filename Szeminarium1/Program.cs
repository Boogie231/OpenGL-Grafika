using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1_24_02_17_2
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;

        private static GL Gl;

        private static uint program;


        private static GlCube glCubeCentered;

        private static GlCube glCubeRotating;

        private static GlCube[, ,] glTeszt= new GlCube[3, 3, 3];

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";
        private const string UpTurnVariableName = "uUpTurn";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;
        uniform mat4 uUpTurn;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
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
            windowOptions.Title = "2 szeminárium";
            windowOptions.Size = new Vector2D<int>(500, 500);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            IInputContext inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl = window.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.White);

            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void LinkProgram()
        {
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
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    cameraDescriptor.DecreaseZYAngle();
                    break;
                    ;
                case Key.Right:
                    cameraDescriptor.IncreaseZYAngle();
                    break;
                case Key.Down:
                    cameraDescriptor.IncreaseDistance();
                    break;
                case Key.Up:
                    cameraDescriptor.DecreaseDistance();
                    break;
                case Key.U:
                    cameraDescriptor.IncreaseZXAngle();
                    break;
                case Key.J:
                    cameraDescriptor.DecreaseZXAngle();
                    break;
                //case Key.Space:
                //    cubeArrangementModel.AnimationEnabeld = !cubeArrangementModel.AnimationEnabeld;
                //    break;
                case Key.W:
                    cameraDescriptor.IncreaseCameraY();
                    break;
                case Key.S:
                    cameraDescriptor.DecreaseCameraY();
                    break;
                case Key.D:
                    cameraDescriptor.IncreaseCameraX();
                    break;
                case Key.A:
                    cameraDescriptor.DecreaseCameraX();
                    break;

                case Key.R:
                    cameraDescriptor.IncreaseCameraZ();
                    break;
                case Key.F:
                    cameraDescriptor.DecreaseCameraZ();
                    break;
                case Key.Space:
                    cubeArrangementModel.Rotate_Upper_clockwise();
                    break;
                case Key.Backspace:
                    cubeArrangementModel.Rotate_Upper_contraclockwise();
                    break;


            }
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls
            cubeArrangementModel.AdvanceTime(deltaTime);

            // TO DO
            cubeArrangementModel.Update_Rotation();
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s].");

            // GL here
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit); // a melysegelesseghez ezeket meg kell kerni hogy szamoljak uujra a szineket s a melysegeket


            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            //DrawPulsingCenterCube();
            //DrawRevolvingCube();

            float a = 0.12f;
            int max = 1;
            for (int i = -1; i <= max; i ++)
            {
                for(int j = -1; j <= max; j++)
                {
                    for(int k = -1; k <= max; k++)
                    {
                        double forg = (j == -1) ? cubeArrangementModel.RotateUpperLayer_current : 0;
                        Draw_Rubick(glTeszt[i + 1, k + 1 , j + 1], [i*a, -j*a, -k*a], forg);

                    }
                }
            }
            
            
        }
        private static unsafe void Draw_Rubick(GlCube cube, float[] translation, double forg)
        {

            // itt egy resze csak gyom még egyelőre
            Matrix4X4<float> diamondScale = Matrix4X4.CreateScale(0.1f); // ez kell

            Matrix4X4<float> rotx = Matrix4X4.CreateRotationX((float)Math.PI / 4f*0);
            Matrix4X4<float> rotz = Matrix4X4.CreateRotationZ((float)Math.PI / 4f * 0);
            //Matrix4X4<float> rotLocY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeAngleOwnRevolution * 0);
            Matrix4X4<float> rotLocY = Matrix4X4.CreateRotationY((float)Math.PI / 2f * 0);
            Matrix4X4<float> trans = Matrix4X4.CreateTranslation(translation[0], translation[1], translation[2]); // ez kell
            Matrix4X4<float> rotGlobY = Matrix4X4.CreateRotationY((float)forg);
            Matrix4X4<float> modelMatrix = diamondScale * rotx * rotz * rotLocY * trans * rotGlobY;

            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(cube.Vao);
            Gl.DrawElements(GLEnum.Triangles, cube.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }


        private static unsafe void DrawRevolvingCube()
        {
            Matrix4X4<float> diamondScale = Matrix4X4.CreateScale(0.25f);
            Matrix4X4<float> rotx = Matrix4X4.CreateRotationX((float)Math.PI / 4f);
            Matrix4X4<float> rotz = Matrix4X4.CreateRotationZ((float)Math.PI / 4f);
            Matrix4X4<float> rotLocY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeAngleOwnRevolution);
            Matrix4X4<float> trans = Matrix4X4.CreateTranslation(1f, 1f, 0f);
            Matrix4X4<float> rotGlobY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeAngleRevolutionOnGlobalY);
            Matrix4X4<float> modelMatrix = diamondScale * rotx * rotz * rotLocY * trans * rotGlobY;

            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(glCubeRotating.Vao);
            Gl.DrawElements(GLEnum.Triangles, glCubeRotating.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void DrawPulsingCenterCube()
        {
            var modelMatrixForCenterCube = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale);

            SetModelMatrix(modelMatrixForCenterCube);
            Gl.BindVertexArray(glCubeCentered.Vao);
            Gl.DrawElements(GLEnum.Triangles, glCubeCentered.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();
        }
        

        private static unsafe void SetUpObjects()
        {

            float[] piros = [1.0f, 0.0f, 0.0f, 1.0f];
            float[] zold = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] kek = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] narancs = [1.0f, 0.5f, 0.0f, 1.0f];
            float[] cian = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] sarga = [1.0f, 1.0f, 0.0f, 1.0f];
            float[] feher = [1.0f, 1.0f, 0.8f, 1.0f];
            float[] fekete = [0.0f, 0.0f, 0.0f, 1.0f];

            //glCubeCentered = GlCube.CreateCubeWithFaceColors(Gl, face1Color, face2Color, face3Color, face4Color, face5Color, face6Color);

            //glCubeRotating = GlCube.CreateCubeWithFaceColors(Gl, face1Color, face2Color, face3Color, face4Color, face5Color, face6Color);

      
            // felső reteg
            glTeszt[0, 0, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, piros, zold, fekete, fekete, fekete);
            glTeszt[1, 0, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, piros, fekete, fekete, fekete, fekete);
            glTeszt[2, 0, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, piros, fekete, fekete, fekete, kek);

            glTeszt[0, 1, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, fekete, zold, fekete, fekete, fekete);
            glTeszt[1, 1, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, fekete, fekete, fekete, fekete, fekete);
            glTeszt[2, 1, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, fekete, fekete, fekete, fekete, kek);
            
            glTeszt[0, 2, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, fekete, zold, fekete, narancs, fekete);
            glTeszt[1, 2, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, fekete, fekete, fekete, narancs, fekete);
            glTeszt[2, 2, 0] = GlCube.CreateCubeWithFaceColors(Gl, feher, fekete, fekete, fekete, narancs, kek);


            // kozepso reteg
            glTeszt[0, 0, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, piros, zold, fekete, fekete, fekete);
            glTeszt[1, 0, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, piros, fekete, fekete, fekete, fekete);
            glTeszt[2, 0, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, piros, fekete, fekete, fekete, kek);

            glTeszt[0, 1, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, zold, fekete, fekete, fekete);
            glTeszt[1, 1, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, fekete, fekete, fekete);
            glTeszt[2, 1, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, fekete, fekete, kek);

            glTeszt[0, 2, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, zold, fekete, narancs, fekete);
            glTeszt[1, 2, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, fekete, narancs, fekete);
            glTeszt[2, 2, 1] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, fekete, narancs, kek);


            // also reteg
            glTeszt[0, 0, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, piros, zold, sarga, fekete, fekete);
            glTeszt[1, 0, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, piros, fekete, sarga, fekete, fekete);
            glTeszt[2, 0, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, piros, fekete, sarga, fekete, kek);

            glTeszt[0, 1, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, zold, sarga, fekete, fekete);
            glTeszt[1, 1, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, sarga, fekete, fekete);
            glTeszt[2, 1, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, sarga, fekete, kek);

            glTeszt[0, 2, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, zold, sarga, narancs, fekete);
            glTeszt[1, 2, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, sarga, narancs, fekete);
            glTeszt[2, 2, 2] = GlCube.CreateCubeWithFaceColors(Gl, fekete, fekete, fekete, sarga, narancs, kek);

        }



        private static void Window_Closing()
        {
            //glCubeCentered.ReleaseGlCube();
            //glCubeRotating.ReleaseGlCube();
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 100);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}