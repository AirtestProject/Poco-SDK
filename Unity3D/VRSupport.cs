using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TcpServer;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class VRSupport
{
    private static Queue<Action> commands = new Queue<Action>();

    public VRSupport()
    {
        commands = new Queue<Action>();
    }

    public void ClearCommands()
    {
        commands.Clear();
    }

    public void PeekCommand()
    {
        if (null != commands && commands.Count > 0)
        {
            Debug.Log("command executed " + commands.Count);
            commands.Peek()();
        }
    }

    public object isVRSupported(List<object> param)
    {
#if UNITY_3 || UNITY_4
			return false;
#elif UNITY_5 || UNITY_2017_1
			return UnityEngine.VR.VRSettings.loadedDeviceName.Equals("CARDBOARD");
#else
        return UnityEngine.XR.XRSettings.loadedDeviceName.Equals("CARDBOARD");
#endif
    }

    public object IsQueueEmpty(List<object> param)
    {
        Debug.Log("Checking queue");
        if (commands != null && commands.Count > 0)
        {
            return null;
        }
        else
        {
            Thread.Sleep(1000); // we wait a bit and check again just in case we run in between calls
            if (commands != null && commands.Count > 0)
            {
                return null;
            }
        }

        return commands.Count;
    }

    public object RotateObject(List<object> param)
    {
        var xRotation = Convert.ToSingle(param[0]);
        var yRotation = Convert.ToSingle(param[1]);
        var zRotation = Convert.ToSingle(param[2]);
        float speed = 0f;
        if (param.Count > 5)
            speed = Convert.ToSingle(param[5]);
        Vector3 mousePosition = new Vector3(xRotation, yRotation, zRotation);
        foreach (GameObject cameraContainer in GameObject.FindObjectsOfType<GameObject>())
        {
            if (cameraContainer.name.Equals(param[3]))
            {
                foreach (GameObject cameraFollower in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (cameraFollower.name.Equals(param[4]))
                    {
                        lock (commands)
                        {
                            commands.Enqueue(() => recoverOffset(cameraFollower, cameraContainer, speed));
                        }

                        lock (commands)
                        {
                            var currentRotation = cameraContainer.transform.rotation;
                            commands.Enqueue(() => rotate(cameraContainer, currentRotation, mousePosition, speed));
                        }
                        return true;
                    }
                }

                return true;
            }
        }
        return false;
    }

    public object ObjectLookAt(List<object> param)
    {
        float speed = 0f;
        if (param.Count > 3)
            speed = Convert.ToSingle(param[3]);
        foreach (GameObject toLookAt in GameObject.FindObjectsOfType<GameObject>())
        {
            if (toLookAt.name.Equals(param[0]))
            {
                foreach (GameObject cameraContainer in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (cameraContainer.name.Equals(param[1]))
                    {
                        foreach (GameObject cameraFollower in GameObject.FindObjectsOfType<GameObject>())
                        {
                            if (cameraFollower.name.Equals(param[2]))
                            {
                                lock (commands)
                                {
                                    commands.Enqueue(() => recoverOffset(cameraFollower, cameraContainer, speed));
                                }

                                lock (commands)
                                {
                                    commands.Enqueue(() => objectLookAt(cameraContainer, toLookAt, speed));
                                }

                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    protected void rotate(GameObject go, Quaternion originalRotation, Vector3 mousePosition, float speed)
    {
        Debug.Log("rotating");
        if (!RotateObject(originalRotation, mousePosition, go, speed))
        {
            lock (commands)
            {
                commands.Dequeue();
            }
        }
    }

    protected void objectLookAt(GameObject go, GameObject toLookAt, float speed)
    {
        Debug.Log("looking at " + toLookAt.name);
        Debug.Log("from " + go.name);
        if (!ObjectLookAtObject(toLookAt, go, speed))
        {
            lock (commands)
            {
                commands.Dequeue();
            }
        }
    }

    protected void recoverOffset(GameObject subcontainter, GameObject cameraContainer, float speed)
    {
        Debug.Log("recovering " + cameraContainer.name);
        if (!ObjectRecoverOffset(subcontainter, cameraContainer, speed))
        {
            lock (commands)
            {
                commands.Dequeue();
            }
        }
    }

    protected bool RotateObject(Quaternion originalPosition, Vector3 mousePosition, GameObject cameraContainer, float rotationSpeed = 0.125f)
    {
        if (null == cameraContainer)
        {
            return false;
        }

        var expectedx = originalPosition.eulerAngles.x + mousePosition.x;
        var expectedy = originalPosition.eulerAngles.y + mousePosition.y;
        var expectedz = originalPosition.eulerAngles.z + mousePosition.z;

        var toRotation = Quaternion.Euler(new Vector3(expectedx, expectedy, expectedz));
        cameraContainer.transform.rotation = Quaternion.Lerp(cameraContainer.transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        var angle = Quaternion.Angle(cameraContainer.transform.rotation, toRotation);

        if (angle == 0)
        {
            return false;
        }

        return true;
    }

    protected bool ObjectLookAtObject(GameObject go, GameObject cameraContainer, float rotationSpeed = 0.125f)
    {
        if (null == go || null == cameraContainer)
        {
            Debug.Log("exception - item null");
            return false;
        }

        var toRotation = Quaternion.LookRotation(go.transform.position - (cameraContainer.transform.localPosition));
        cameraContainer.transform.rotation = Quaternion.Lerp(cameraContainer.transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        // It should not be needed but sometimes the difference of eurlerAngles might be small and this would ensure it works fine
        if (Quaternion.Angle(cameraContainer.transform.rotation, toRotation) == 0)
        {

            return false;
        }

        return true;
    }

    protected bool ObjectRecoverOffset(GameObject subcontainer, GameObject cameraContainer, float rotationSpeed = 0.125f)
    {
        if (null == cameraContainer)
        {
            Debug.Log("exception - item null");
            return false;
        }

        // add offset with the camera
        var cameraRotation = Camera.main.transform.localRotation;

        var toRotate = new Quaternion(-cameraRotation.x, -cameraRotation.y, -cameraRotation.z, cameraRotation.w);
        subcontainer.transform.localRotation = Quaternion.Lerp(subcontainer.transform.localRotation, toRotate, rotationSpeed * Time.deltaTime);

        // It should not be needed but sometimes the difference of eurlerAngles might be small and this would ensure it works fine
        if (Quaternion.Angle(subcontainer.transform.localRotation, toRotate) == 0)
        {
            return false;
        }

        return true;
    }
}