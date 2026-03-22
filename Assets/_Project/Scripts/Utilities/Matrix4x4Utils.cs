using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for Matrix4x4 operations
    /// </summary>
    public static class Matrix4x4Utils
    {
        /// <summary>
        /// Creates a translation matrix
        /// </summary>
        public static Matrix4x4 Translation(Vector3 position)
        {
            return Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Creates a rotation matrix
        /// </summary>
        public static Matrix4x4 Rotation(Quaternion rotation)
        {
            return Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        }

        /// <summary>
        /// Creates a scaling matrix
        /// </summary>
        public static Matrix4x4 Scale(Vector3 scale)
        {
            return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        }

        /// <summary>
        /// Creates a TRS matrix (translation, rotation, scale)
        /// </summary>
        public static Matrix4x4 TRS(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return Matrix4x4.TRS(position, rotation, scale);
        }

        /// <summary>
        /// Gets the position from a matrix
        /// </summary>
        public static Vector3 GetPosition(Matrix4x4 matrix)
        {
            return matrix.GetColumn(3);
        }

        /// <summary>
        /// Gets the rotation from a matrix
        /// </summary>
        public static Quaternion GetRotation(Matrix4x4 matrix)
        {
            return matrix.rotation;
        }

        /// <summary>
        /// Gets the scale from a matrix
        /// </summary>
        public static Vector3 GetScale(Matrix4x4 matrix)
        {
            return matrix.lossyScale;
        }

        /// <summary>
        /// Inverts a matrix
        /// </summary>
        public static Matrix4x4 Inverse(Matrix4x4 matrix)
        {
            return matrix.inverse;
        }

        /// <summary>
        /// Transposes a matrix
        /// </summary>
        public static Matrix4x4 Transpose(Matrix4x4 matrix)
        {
            return matrix.transpose;
        }

        /// <summary>
        /// Multiplies two matrices
        /// </summary>
        public static Matrix4x4 Multiply(Matrix4x4 a, Matrix4x4 b)
        {
            return a * b;
        }

        /// <summary>
        /// Transforms a point by a matrix
        /// </summary>
        public static Vector3 TransformPoint(Matrix4x4 matrix, Vector3 point)
        {
            return matrix.MultiplyPoint3x4(point);
        }

        /// <summary>
        /// Transforms a direction by a matrix
        /// </summary>
        public static Vector3 TransformDirection(Matrix4x4 matrix, Vector3 direction)
        {
            return matrix.MultiplyVector(direction);
        }

        /// <summary>
        /// Creates a look-at matrix
        /// </summary>
        public static Matrix4x4 LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            return Matrix4x4.LookAt(eye, target, up);
        }

        /// <summary>
        /// Creates an orthographic projection matrix
        /// </summary>
        public static Matrix4x4 Ortho(float left, float right, float bottom, float top, float near, float far)
        {
            return Matrix4x4.Ortho(left, right, bottom, top, near, far);
        }

        /// <summary>
        /// Creates a perspective projection matrix
        /// </summary>
        public static Matrix4x4 Perspective(float fov, float aspect, float near, float far)
        {
            return Matrix4x4.Perspective(fov, aspect, near, far);
        }

        /// <summary>
        /// Creates a scaling matrix around a point
        /// </summary>
        public static Matrix4x4 ScaleAroundPoint(Vector3 scale, Vector3 point)
        {
            return Translation(point) * Scale(scale) * Translation(-point);
        }

        /// <summary>
        /// Creates a rotation matrix around a point
        /// </summary>
        public static Matrix4x4 RotateAroundPoint(Quaternion rotation, Vector3 point)
        {
            return Translation(point) * Rotation(rotation) * Translation(-point);
        }

        /// <summary>
        /// Linearly interpolates between two matrices
        /// </summary>
        public static Matrix4x4 Lerp(Matrix4x4 a, Matrix4x4 b, float t)
        {
            Matrix4x4 result = new Matrix4x4();
            for (int i = 0; i < 16; i++)
            {
                result[i] = Mathf.Lerp(a[i], b[i], t);
            }
            return result;
        }

        /// <summary>
        /// Linearly interpolates between two matrices (unclamped)
        /// </summary>
        public static Matrix4x4 LerpUnclamped(Matrix4x4 a, Matrix4x4 b, float t)
        {
            Matrix4x4 result = new Matrix4x4();
            for (int i = 0; i < 16; i++)
            {
                result[i] = Mathf.LerpUnclamped(a[i], b[i], t);
            }
            return result;
        }

        /// <summary>
        /// Checks if two matrices are approximately equal
        /// </summary>
        public static bool Approximately(Matrix4x4 a, Matrix4x4 b, float tolerance = 0.001f)
        {
            for (int i = 0; i < 16; i++)
            {
                if (Mathf.Abs(a[i] - b[i]) > tolerance)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the determinant of a matrix
        /// </summary>
        public static float Determinant(Matrix4x4 matrix)
        {
            return matrix.determinant;
        }

        /// <summary>
        /// Gets if a matrix is invertible
        /// </summary>
        public static bool IsInvertible(Matrix4x4 matrix)
        {
            return matrix.determinant != 0f;
        }
    }
}
