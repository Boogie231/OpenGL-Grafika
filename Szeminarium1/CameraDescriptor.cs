using Silk.NET.Maths;

namespace Szeminarium1_24_02_17_2
{
    internal class CameraDescriptor
    {
        private double DistanceToOrigin = 1.6; // sajat izlesnek àtirt, erdekes szog

        private double AngleToZYPlane = 0.7;

        private double AngleToZXPlane = 0.7;

        private const double DistanceScaleFactor = 1.1;

        private const double AngleChangeStepSize = Math.PI / 180 * 5;

        private const float DistToMoveCamera = 0.1f;

        private float x = 0f;
        private float y = 0f;
        private float z = 0f;

        /// <summary>
        /// Gets the position of the camera.
        /// </summary>
        public Vector3D<float> Position
        {
            get
            {
                return GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
            }
        }

        /// <summary>
        /// Gets the up vector of the camera.
        /// </summary>
        public Vector3D<float> UpVector
        {
            get
            {
                return Vector3D.Normalize(GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane + Math.PI / 2));
            }
        }

        /// <summary>
        /// Gets the target point of the camera view.
        /// </summary>
        public Vector3D<float> Target
        {
            get
            {
                // For the moment the camera is always pointed at the origin.
                Vector3D<float> vektor = new Vector3D<float>(x, y, z);
                return vektor;
            }
        }

        public void IncreaseZXAngle()
        {
            AngleToZXPlane += AngleChangeStepSize;
        }

        public void DecreaseZXAngle()
        {
            AngleToZXPlane -= AngleChangeStepSize;
        }

        public void IncreaseZYAngle()
        {
            AngleToZYPlane += AngleChangeStepSize;

        }

        public void DecreaseZYAngle()
        {
            AngleToZYPlane -= AngleChangeStepSize;
        }

        public void IncreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin * DistanceScaleFactor;
        }

        public void DecreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin / DistanceScaleFactor;
        }

        public void IncreaseCameraX()
        {
            x += DistToMoveCamera;
        }

        public void DecreaseCameraX()
        {
            x -= DistToMoveCamera;
        }

        public void IncreaseCameraY()
        {
            y += DistToMoveCamera;
        }

        public void DecreaseCameraY()
        {
            y -= DistToMoveCamera;
        }

        public void IncreaseCameraZ()
        {
            z += DistToMoveCamera;
        }

        public void DecreaseCameraZ()
        {
            z -= DistToMoveCamera;
        }


        private static Vector3D<float> GetPointFromAngles(double distanceToOrigin, double angleToMinZYPlane, double angleToMinZXPlane)
        {
            var x = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Sin(angleToMinZYPlane);
            var z = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Cos(angleToMinZYPlane);
            var y = distanceToOrigin * Math.Sin(angleToMinZXPlane);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }
    }
}
