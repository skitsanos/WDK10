Imports System.Xml

Public Class CsvToXmlConvertor

#Region " Properties "
    Private _Delimeter As String = vbTab
    Public Property Delimeter() As String
        Get
            Return _Delimeter
        End Get
        Set(ByVal value As String)
            _Delimeter = value
        End Set
    End Property

    Private _hasFieldNames As Boolean = True
    Public Property HasFieldNamses() As Boolean
        Get
            Return _hasFieldNames
        End Get
        Set(ByVal value As Boolean)
            _hasFieldNames = value
        End Set
    End Property
#End Region

    Public Function Convert(ByVal source As String, ByVal rowName As String, Optional ByVal columns As String() = Nothing) As String
        'If HasFieldNamses = False And columns IsNot Nothing Then
        '    Throw New Exception("At least one data column name required")
        '    Exit Function
        'End If

        Dim ds As New Data.DataSet("XmlDataSet")

        Dim dt As New Data.DataTable(rowName)
        ds.Tables.Add(dt)

        Dim sr As New IO.StreamReader(source)
        sr.BaseStream.Seek(0, IO.SeekOrigin.Begin)

        If HasFieldNamses = False Then
            For Each columnName As String In columns
                columnName = columnName.Replace(" ", "_")
                dt.Columns.Add(columnName)
            Next
        Else
            For Each field As String In sr.ReadLine.Split(Delimeter)
                field = field.Replace(" ", "_")
                dt.Columns.Add(field)
            Next
        End If

        dt.Columns.Add("uid")

        Debug.WriteLine(dt.Columns.Count)

        Dim counter As Integer = 0
        Dim rowId As Integer = 0
        While sr.Peek > -1
            Dim dataRow As Data.DataRow = dt.NewRow

            For Each rowField As String In sr.ReadLine.Split(Delimeter)
                dataRow(counter) = rowField
                counter += 1
            Next

            dataRow("uid") = rowId

            dt.Rows.Add(dataRow)

            counter = 0
            rowId += 1
        End While

        Dim ms As New IO.MemoryStream
        dt.WriteXml(ms)

        ms.Seek(0, IO.SeekOrigin.Begin)

        sr.Close()

        Return Text.Encoding.UTF8.GetString(ms.GetBuffer())
    End Function
End Class
