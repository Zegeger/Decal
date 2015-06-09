<ComClass(WindowOutput.ClassId, WindowOutput.InterfaceId, WindowOutput.EventsId)> _
Public Class WindowOutput

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "3224dc9f-009a-48bc-8e4c-d21c758147e0"
    Public Const InterfaceId As String = "c8ff04ba-d53f-4b02-82f6-ebebe61c5f24"
    Public Const EventsId As String = "33aeecfb-c7bf-4f8c-a56b-62773e6268dc"
#End Region

    Friend windowsChanged As Boolean
    Friend baseMainWindow As Boolean
    Friend baseWindow1 As Boolean
    Friend baseWindow2 As Boolean
    Friend baseWindow3 As Boolean
    Friend baseWindow4 As Boolean

    Public Sub New()
        MyBase.New()
        windowsChanged = False
    End Sub

    Public Property MainWindow() As Boolean
        Get
            Return baseMainWindow
        End Get
        Set(ByVal value As Boolean)
            baseMainWindow = value
            windowsChanged = True
        End Set
    End Property

    Public Property Window1() As Boolean
        Get
            Return baseWindow1
        End Get
        Set(ByVal value As Boolean)
            baseWindow1 = value
            windowsChanged = True
        End Set
    End Property

    Public Property Window2() As Boolean
        Get
            Return baseWindow2
        End Get
        Set(ByVal value As Boolean)
            baseWindow2 = value
            windowsChanged = True
        End Set
    End Property

    Public Property Window3() As Boolean
        Get
            Return baseWindow3
        End Get
        Set(ByVal value As Boolean)
            baseWindow3 = value
            windowsChanged = True
        End Set
    End Property

    Public Property Window4() As Boolean
        Get
            Return baseWindow4
        End Get
        Set(ByVal value As Boolean)
            baseWindow4 = value
            windowsChanged = True
        End Set
    End Property

    Public Sub ForceDefault()
        windowsChanged = True
    End Sub

    Public Sub BlockAll()
        MainWindow = False
        Window1 = False
        Window2 = False
        Window3 = False
        Window4 = False
    End Sub

    Public Sub SetWindows(ByVal Main As Boolean, ByVal W1 As Boolean, ByVal W2 As Boolean, ByVal W3 As Boolean, ByVal W4 As Boolean)
        MainWindow = Main
        Window1 = W1
        Window2 = W2
        Window3 = W3
        Window4 = W4
    End Sub

End Class


