<ComClass(ChatBoxMessageEventArgs.ClassId, ChatBoxMessageEventArgs.InterfaceId, ChatBoxMessageEventArgs.EventsId)> _
Public Class ChatBoxMessageEventArgs

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "855d77e3-c4cf-4daa-b666-95b1bda424be"
    Public Const InterfaceId As String = "fb5a4a41-3978-4543-b3ca-b940ed22ae9e"
    Public Const EventsId As String = "3ecd59e3-dfc7-47fa-bfa9-0b5d2c65d67d"
#End Region

    Friend ie As Zegeger.Decal.Plugins.ZChatSystem.ChatBoxMessageEventArgs
    Friend baseWindowOutput As WindowOutput

    Friend baseMessageID As Integer
    Friend baseColor As Integer
    Friend colorChanged As Boolean
    'Friend basePluginID As String
    Friend baseText As String
    Friend baseType As String


    Public Sub New()
        MyBase.New()
        colorChanged = False
    End Sub

    Public ReadOnly Property MessageID() As Integer
        Get
            Return baseMessageID
        End Get
    End Property

    'Public WriteOnly Property PluginID() As String
    '    Set(ByVal value As String)
    '        basePluginID = value
    '    End Set
    'End Property

    Public ReadOnly Property Text() As String
        Get
            Return baseText
        End Get
    End Property
    Public Property Color() As Integer
        Get
            Return baseColor
        End Get
        Set(ByVal value As Integer)
            baseColor = value
            colorChanged = True
        End Set
    End Property

    Public ReadOnly Property Type() As Integer
        Get
            Return baseType
        End Get
    End Property

    Public ReadOnly Property Window() As WindowOutput
        Get
            Return baseWindowOutput
        End Get
    End Property

    Public Sub ReplaceMessage(ByVal Find As String, ByVal Replace As String)
        ie.ReplaceMessage(Find, Replace)
    End Sub

    Public Sub SubstituteMessage(ByVal NewMessage As String)
        ie.SubstituteMessage(NewMessage)
    End Sub

    Public Sub AppendMessage(ByVal AppendText As String)
        ie.AppendMessage(AppendText)
    End Sub

    Public Sub PrependMessage(ByVal PrependText As String)
        ie.PrependMessage(PrependText)
    End Sub

End Class


