<ComClass(ChatType.ClassId, ChatType.InterfaceId, ChatType.EventsId)> _
Public Class ChatType

#Region "COM GUIDs"
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "9a5cb1c5-f6d7-4d3f-80dc-580ee368ddde"
    Public Const InterfaceId As String = "3ab947e4-ffa3-4ecc-9366-8a0d33676ae5"
    Public Const EventsId As String = "d91ebcb4-c01f-4e2c-a9f9-a726c78c196d"
#End Region

    Public Sub New()
        MyBase.New()
    End Sub

    Friend internalValue As Integer

    Public ReadOnly Property Value() As Integer
        Get
            Return internalValue
        End Get
    End Property
End Class


