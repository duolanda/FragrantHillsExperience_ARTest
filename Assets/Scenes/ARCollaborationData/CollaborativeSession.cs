using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;

#if UNITY_IOS && !UNITY_EDITOR
using Unity.iOS.Multipeer;
using UnityEngine.XR.ARKit;
using System;
#endif

[RequireComponent(typeof(ARSession))]
public class CollaborativeSession : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The name for this network service. It should be 15 characters or less and can contain ASCII, lowercase letters, numbers, and hyphens.")]
    string m_ServiceType;

    /// <summary>
    /// The name for this network service.
    /// See <a href="https://developer.apple.com/documentation/multipeerconnectivity/mcnearbyserviceadvertiser">MCNearbyServiceAdvertiser</a>
    /// for the purpose of and restrictions on this name.
    /// </summary>
    public string serviceType
    {
        get => m_ServiceType;
        set => m_ServiceType = value;
    }

    ARSession m_ARSession;

    void DisableNotSupported(string reason)
    {
        enabled = false;
    }

    void OnEnable()
    {
#if UNITY_IOS && !UNITY_EDITOR
        var subsystem = GetSubsystem();
        if (!ARKitSessionSubsystem.supportsCollaboration || subsystem == null)
        {
            DisableNotSupported("Collaborative sessions require iOS 13.");
            return;
        }

        subsystem.collaborationRequested = true;
        m_MCSession.Enabled = true;
#else
        DisableNotSupported("Collaborative sessions are an ARKit 3 feature; This platform does not support them.");
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    MCSession m_MCSession;

    ARKitSessionSubsystem GetSubsystem()
    {
        if (m_ARSession == null)
            return null;

        return m_ARSession.subsystem as ARKitSessionSubsystem;
    }

    void Awake()
    {
        m_ARSession = GetComponent<ARSession>();
        m_MCSession = new MCSession(SystemInfo.deviceName, m_ServiceType);
    }

    void OnDisable()
    {
        m_MCSession.Enabled = false;

        var subsystem = GetSubsystem();
        if (subsystem != null)
            subsystem.collaborationRequested = false;
    }

    void Update()
    {
        var subsystem = GetSubsystem();
        if (subsystem == null)
            return;

        // Check for new collaboration data
        while (subsystem.collaborationDataCount > 0)
        {
            using (var collaborationData = subsystem.DequeueCollaborationData())
            {
                CollaborationNetworkingIndicator.NotifyHasCollaborationData();

                if (m_MCSession.ConnectedPeerCount == 0)
                    continue;

                using (var serializedData = collaborationData.ToSerialized())
                using (var data = NSData.CreateWithBytesNoCopy(serializedData.bytes))
                {
                    m_MCSession.SendToAllPeers(data, collaborationData.priority == ARCollaborationDataPriority.Critical
                        ? MCSessionSendDataMode.Reliable
                        : MCSessionSendDataMode.Unreliable);

                    CollaborationNetworkingIndicator.NotifyOutgoingDataSent();

                    // Only log 'critical' data as 'optional' data tends to come every frame
                    if (collaborationData.priority == ARCollaborationDataPriority.Critical)
                    {
                        Debug.Log($"Sent {data.Length} bytes of collaboration data.");
                    }
                }
            }
        }

        // Check for incoming data
        while (m_MCSession.ReceivedDataQueueSize > 0)
        {
            CollaborationNetworkingIndicator.NotifyIncomingDataReceived();

            using (var data = m_MCSession.DequeueReceivedData())
            using (var collaborationData = new ARCollaborationData(data.Bytes))
            {
                if (collaborationData.valid)
                {
                    subsystem.UpdateWithCollaborationData(collaborationData);

                    if (collaborationData.priority == ARCollaborationDataPriority.Critical)
                    {
                        Debug.Log($"Received {data.Bytes.Length} bytes of collaboration data.");
                    }
                }
                else
                {
                    //虽然不知道它们 valid 是怎么实现的，但可以根据排除法知道，我们的数据肯定不是 valid
                     NativeSlice<byte> nativeSlice = data.Bytes;
                                    byte[] byteArray = new byte[nativeSlice.Length];
                                    for (int i = 0; i < nativeSlice.Length; i++)
                                    {
                                        byteArray[i] = data.Bytes[i];
                                    }
                    ARDrawManager.Instance.HandleReceiveLinesData(byteArray);

                    Debug.Log($"Received {data.Bytes.Length} bytes from remote, but the collaboration data was not valid.");
                }
            }
        }
    }

    void OnDestroy()
    {
        m_MCSession.Dispose();
    }
    
    public void SendLinesData(byte[] lineData)
    {
        if (m_MCSession.ConnectedPeerCount > 0)
        {
            NativeArray<byte> nativeArray = new NativeArray<byte>(lineData, Allocator.Temp);
            NativeSlice<byte> nativeSlice = new NativeSlice<byte>(nativeArray);
            m_MCSession.SendToAllPeers(NSData.CreateWithBytesNoCopy(nativeSlice), MCSessionSendDataMode.Reliable);
            // nativeArray.Dispose();
        }
    }

    public void ReceiveLinesData(Action<byte[]> onLineDataReceived)
    {
        while (m_MCSession.ReceivedDataQueueSize > 0)
        {
            using (var data = m_MCSession.DequeueReceivedData())
            {
                NativeSlice<byte> nativeSlice = data.Bytes;
                byte[] byteArray = new byte[nativeSlice.Length];
                for (int i = 0; i < nativeSlice.Length; i++)
                {
                    byteArray[i] = data.Bytes[i];
                }
                onLineDataReceived?.Invoke(byteArray);
            }
        }
    }
#endif
}
