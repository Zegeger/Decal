<ComClass(ChatTextCompleteEventArgs.ClassId, ChatTextCompleteEventArgs.InterfaceId, ChatTextCompleteEventArgs.EventsId)> _
Public Class ChatTextCompleteEventArgs

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "7ed8fdfe-6e34-4520-868f-026205f8391e"
    Public Const InterfaceId As String = "10d6696d-d5b9-4266-89ca-31b6826b9fc0"
    Public Const EventsId As String = "9c4f4017-ca68-4117-821c-0972111a8fa8"
#End Region

    Friend baseWindowOutput As CompletedWindowOutput

    Friend baseMessageID As Integer
    Friend baseColor As Integer
    Friend baseText As String
    Friend baseType As Integer

    Public Sub New()
        MyBase.New()
    End Sub

    Public ReadOnly Property MessageID() As Integer
        Get
            Return baseMessageID
        End Get
    End Property

    Public ReadOnly Property Text() As String
        Get
            Return baseText
        End Get
    End Property

    Public ReadOnly Property Color() As Integer
        Get
            Return baseColor
        End Get
    End Property

    Public ReadOnly Property Type() As Integer
        Get
            Return baseType
        End Get
    End Property

    Public ReadOnly Property Window() As CompletedWindowOutput
        Get
            Return baseWindowOutput
        End Get
    End Property



End Class


