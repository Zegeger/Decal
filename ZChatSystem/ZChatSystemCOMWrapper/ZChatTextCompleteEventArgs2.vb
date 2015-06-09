<ComClass(ZChatTextCompleteEventArgs.ClassId, ZChatTextCompleteEventArgs.InterfaceId, ZChatTextCompleteEventArgs.EventsId)> _
Public Class ZChatTextCompleteEventArgs

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "993BFF2D-D288-4082-955F-79E78BB512F7"
    Public Const InterfaceId As String = "3E9B0855-0CA8-46ec-9348-1BB544B92CA7"
    Public Const EventsId As String = "EB15F028-58DA-4453-9ACA-F185517A3886"
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


