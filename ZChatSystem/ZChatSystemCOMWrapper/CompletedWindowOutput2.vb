<ComClass(CompletedWindowOutput.ClassId, CompletedWindowOutput.InterfaceId, CompletedWindowOutput.EventsId)> _
Public Class CompletedWindowOutput

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "6AE96002-87B8-47b2-9F63-3599F2A30F38"
    Public Const InterfaceId As String = "D6077A1D-AC82-4213-895B-3539232BCB7F"
    Public Const EventsId As String = "822037FF-F35C-4c3e-8832-E6264AAADF79"
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


