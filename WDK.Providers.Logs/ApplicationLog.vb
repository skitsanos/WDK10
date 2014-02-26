Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WDK.API.CouchDb
Imports System.Web


Public Class ApplicationLog

#Region " getDatasource() "
    Public Function getDatasource(Optional ByVal query As String = "") As List(Of LogEntryType)
        Dim ret As List(Of LogEntryType) = Nothing

        Dim settings As CouchDbConnectionSettings = getConnectionSettings()
		Dim db As New Database(settings.host, settings.port, settings.username, settings.password)

        ' make sure our design view actually exists
        'If (Not db.documentExists(settings.db, "_design/ApplicationLog")) Then
        '	db.createDesignDocument(settings.db, "ApplicationLog", "showAll", "function(doc){if (doc.type && doc.type == 'LogEntryType') emit(doc.createdOn, doc);}")
        'End If

        ret = db.getDesignView(Of LogEntryType)(settings.db, "ApplicationLog", "showAll")

        Return ret
    End Function
#End Region

#Region " Write() "
    Public Function write(ByVal Data As String, Optional ByVal IsError As Boolean = False) As Boolean
		Dim ret As Boolean = False

        Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

        Dim entry As New LogEntryType
        entry.isError = IsError
        entry.content = Data
        entry.host = HttpContext.Current.Request.Url.Host
        entry.userAgent = HttpContext.Current.Request.UserAgent

        db.createDocument(settings.db, entry)

        Return ret
    End Function
#End Region

#Region " remove() "
    Public Function remove(ByVal Id As String) As Boolean
        Dim ret As Boolean = False

        Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

        db.deleteDocument(settings.db, Id)

        Return ret
    End Function
#End Region

#Region " removeAll() "
    Public Function removeAll() As Boolean
        Dim ret As Boolean = True

        Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

        Dim result As String = db.getDesignViewAsJson(settings.db, "ApplicationLog", "showAll")
        Dim ro As JObject = DirectCast(JsonConvert.DeserializeObject(result), JObject)

        If (ro.Item("rows") IsNot Nothing) Then
            Dim list As New JArray()

            For Each row As JObject In ro.Item("rows")
                row.Add("_id", row.Item("id"))
                row.Remove("id")

                row.Remove("key")

                row.Add("_rev", row.Item("value").Item("_rev"))
                row.Remove("value")

                row.Add("_deleted", True)
                list.Add(row)
            Next

            Dim bulkDeleteCommand As New JObject()
            bulkDeleteCommand.Add("docs", list)

            db.deleteDocuments(settings.db, JsonConvert.SerializeObject(bulkDeleteCommand))
        End If

        Return ret
    End Function
#End Region

End Class