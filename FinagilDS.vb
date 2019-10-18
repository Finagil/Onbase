Imports System.Data.SqlClient
Partial Class FinagilDS
    
End Class

Namespace FinagilDSTableAdapters
    Partial Public Class OnBaseTableAdapter
        Public Property Conecciones As String
            Get
                Return Me.CommandCollection(0).Connection.ConnectionString
            End Get
            Set(value As String)
                For Each cmd As SqlCommand In Me.CommandCollection
                    cmd.Connection.ConnectionString = value
                Next
            End Set
        End Property

        Public Property TimeOuts As Integer
            Get
                Return Me.CommandCollection(0).Connection.ConnectionTimeout
            End Get
            Set(value As Integer)
                For Each cmd As SqlCommand In Me.CommandCollection
                    cmd.CommandTimeout = value
                Next
            End Set
        End Property
    End Class
End Namespace
