<ComClass(IDToken.ClassId, IDToken.InterfaceId, IDToken.EventsId)> _
Public Class IDToken

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "3349e17f-b6be-43c2-b2bb-ce7454dc5544"
    Public Const InterfaceId As String = "a29d4567-6d34-4e82-9d0b-27645c3c9f2b"
    Public Const EventsId As String = "638f3e27-21a4-4a47-83eb-48f658b090bd"
#End Region

    ' A creatable COM class must have a Public Sub New() 
    ' with no parameters, otherwise, the class will not be 
    ' registered in the COM registry and cannot be created 
    ' via CreateObject.
    Public Sub New()
        MyBase.New()
    End Sub

    Friend zID As Guid

    Public ReadOnly Property ID() As String
        Get
            Return zID.ToString()
        End Get
    End Property
End Class


