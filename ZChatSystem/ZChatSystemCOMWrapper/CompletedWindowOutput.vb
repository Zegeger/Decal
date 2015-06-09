<ComClass(CompletedWindowOutput.ClassId, CompletedWindowOutput.InterfaceId, CompletedWindowOutput.EventsId)> _
Public Class CompletedWindowOutput

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "7afbb02a-770d-425b-ab0e-b8703a131cb8"
    Public Const InterfaceId As String = "a89c7a23-4ba5-4d38-9259-1db97cd3df07"
    Public Const EventsId As String = "2f5bf8bc-6a3b-4919-b1fb-e45f435a0cac"
#End Region

    Friend baseMainWindow As Boolean
    Friend baseWindow1 As Boolean
    Friend baseWindow2 As Boolean
    Friend baseWindow3 As Boolean
    Friend baseWindow4 As Boolean

    Public Sub New()
        MyBase.New()
    End Sub

    Public ReadOnly Property MainWindow() As Boolean
        Get
            Return baseMainWindow
        End Get
    End Property

    Public ReadOnly Property Window1() As Boolean
        Get
            Return baseWindow1
        End Get
    End Property

    Public ReadOnly Property Window2() As Boolean
        Get
            Return baseWindow2
        End Get
    End Property

    Public ReadOnly Property Window3() As Boolean
        Get
            Return baseWindow3
        End Get
    End Property

    Public ReadOnly Property Window4() As Boolean
        Get
            Return baseWindow4
        End Get
    End Property

End Class


