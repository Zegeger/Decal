Imports System.Runtime.InteropServices

'<Guid("A6B76F24-A242-443a-9714-3B8A5B2448F1"), _
'InterfaceType(ComInterfaceType.InterfaceIsIDispatch)> _
'Public Interface IZChatSystemCOMWrapper
'<DispId(1)> _
'Sub ChatBoxMessage()

'<DispId(2)> _
'Sub ChatTextComplete()
'End Interface

<ComClass(Wrapper.ClassId, Wrapper.InterfaceId, Wrapper.EventsId)> _
Public Class Wrapper

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "3896a34d-a61e-4b1b-9054-4ecc525c4731"
    Public Const InterfaceId As String = "ecccac87-7b9b-43aa-a59b-f26312ae78b5"
    Public Const EventsId As String = "bfac0968-ff81-4762-b148-a5618c617c4a"
#End Region

    Public Delegate Sub ChatBoxMessageCOMEvent(ByVal e As ChatBoxMessageEventArgs)
    Public Delegate Sub ChatTextCOMComplete(ByVal e As ChatTextCompleteEventArgs)

    Public Event ChatBoxMessage As ChatBoxMessageCOMEvent
    Public Event ChatTextComplete As ChatTextCOMComplete

    Private instID As Zegeger.Decal.Plugins.ZChatSystem.IDToken
    Private Types As Dictionary(Of Integer, Zegeger.Decal.Plugins.ZChatSystem.ChatType)

    Public Sub RegisterPlugin(ByVal PluginName As String)
        instID = Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.RegisterPlugin(PluginName)
    End Sub

    Public Sub UnregisterPlugin()
        ZChat = Nothing
        instID = Nothing
        Types = Nothing
    End Sub

    Public Function CreateChatType(ByVal Color As Integer, ByVal MainWindow As Boolean, ByVal Window1 As Boolean, ByVal Window2 As Boolean, ByVal Window3 As Boolean, ByVal Window4 As Boolean) As ChatType
        Dim zType As Zegeger.Decal.Plugins.ZChatSystem.ChatType
        zType = Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.CreateChatType(instID, Color, MainWindow, Window1, Window2, Window3, Window4)

        Types.Add(zType.Value, zType)

        Dim tmp As ChatType
        tmp = New ChatType
        tmp.internalValue = zType.Value

        Return tmp
    End Function

    Public Sub WriteToChat(ByVal Text As String, ByVal Color As Integer)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, Color)
    End Sub

    Public Sub WriteToChat(ByVal Text As String, ByVal Color As Integer, ByVal Target As Integer)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, Color, Target)
    End Sub

    Public Sub WriteToChat(ByVal Text As String, ByVal Color As Integer, ByVal Target As WindowOutput)
        Dim g As Zegeger.Decal.Plugins.ZChatSystem.WindowOutput
        g = New Zegeger.Decal.Plugins.ZChatSystem.WindowOutput
        g.SetWindows(Target.baseMainWindow, Target.baseWindow1, Target.baseWindow2, Target.baseWindow3, Target.baseWindow4)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, Color, g)
    End Sub

    Public Sub WriteToChat(ByVal Text As String, ByRef Type As ChatType)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, Types(Type.internalValue))
    End Sub

    Public Sub WriteToChat(ByVal Text As String, ByVal Color As Integer, ByRef Type As ChatType)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, Color, Types(Type.internalValue))
    End Sub

    Public Sub WriteToChat(ByVal Text As String, ByVal Target As WindowOutput, ByRef Type As ChatType)
        Dim g As Zegeger.Decal.Plugins.ZChatSystem.WindowOutput
        g = New Zegeger.Decal.Plugins.ZChatSystem.WindowOutput
        g.SetWindows(Target.baseMainWindow, Target.baseWindow1, Target.baseWindow2, Target.baseWindow3, Target.baseWindow4)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, g, Types(Type.internalValue))
    End Sub

    Public Sub WriteToChat(ByVal Text As String, ByVal Color As Integer, ByVal Target As Integer, ByRef Type As ChatType)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, Color, Target, Types(Type.internalValue))
    End Sub

    Public Sub WriteToChat(ByVal Text As String, ByVal Color As Integer, ByVal Target As WindowOutput, ByRef Type As ChatType)
        Dim g As Zegeger.Decal.Plugins.ZChatSystem.WindowOutput
        g = New Zegeger.Decal.Plugins.ZChatSystem.WindowOutput
        g.SetWindows(Target.baseMainWindow, Target.baseWindow1, Target.baseWindow2, Target.baseWindow3, Target.baseWindow4)
        Call Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem.WriteToChat(Text, Color, g, Types(Type.internalValue))
    End Sub

    Private WithEvents ZChat As Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem

    Public Sub New()
        MyBase.New()
        Call Init()
    End Sub

    Public Sub New(ByVal PluginName As String)
        MyBase.New()
        Call Init()
        RegisterPlugin(PluginName)
    End Sub

    Private Sub Init()
        Types = New Dictionary(Of Integer, Zegeger.Decal.Plugins.ZChatSystem.ChatType)
        ZChat = New Zegeger.Decal.Plugins.ZChatSystem.ZChatSystem()
    End Sub

    Protected Overloads Overrides Sub Finalize()
        MyBase.Finalize()
        ZChat = Nothing
        Types = Nothing
    End Sub 'Finalize

    Private Sub ZChat_ChatBoxMessage(ByVal e As Zegeger.Decal.Plugins.ZChatSystem.ChatBoxMessageEventArgs) Handles ZChat.ChatBoxMessage
        Dim f As ChatBoxMessageEventArgs
        f = New ChatBoxMessageEventArgs

        Dim g As WindowOutput
        g = New WindowOutput

        g.baseMainWindow = e.Window.MainWindow
        g.baseWindow1 = e.Window.Window1
        g.baseWindow2 = e.Window.Window2
        g.baseWindow3 = e.Window.Window3
        g.baseWindow4 = e.Window.Window4

        f.baseWindowOutput = g
        f.ie = e
        f.baseColor = e.Color
        f.baseType = e.Type
        f.baseMessageID = e.MessageID
        f.baseText = e.Text

        RaiseEvent ChatBoxMessage(f)

        If f.colorChanged Then
            e.Color = f.baseColor
        End If
        If f.baseWindowOutput.windowsChanged Then
            e.Window.MainWindow = f.baseWindowOutput.baseMainWindow
            e.Window.Window1 = f.baseWindowOutput.baseWindow1
            e.Window.Window2 = f.baseWindowOutput.baseWindow2
            e.Window.Window3 = f.baseWindowOutput.baseWindow3
            e.Window.Window4 = f.baseWindowOutput.baseWindow4
        End If
        e.PluginID = instID

    End Sub

    Private Sub ZChat_ChatTextComplete(ByVal e As Zegeger.Decal.Plugins.ZChatSystem.ChatTextCompleteEventArgs) Handles ZChat.ChatTextComplete
        Dim f As ChatTextCompleteEventArgs
        f = New ChatTextCompleteEventArgs

        Dim g As CompletedWindowOutput
        g = New CompletedWindowOutput

        g.baseMainWindow = e.Window.MainWindow
        g.baseWindow1 = e.Window.Window1
        g.baseWindow2 = e.Window.Window2
        g.baseWindow3 = e.Window.Window3
        g.baseWindow4 = e.Window.Window4

        f.baseWindowOutput = g
        f.baseColor = e.Color
        f.baseText = e.Text
        f.baseMessageID = e.MessageID
        f.baseType = e.Type

        RaiseEvent ChatTextComplete(f)
    End Sub
End Class
